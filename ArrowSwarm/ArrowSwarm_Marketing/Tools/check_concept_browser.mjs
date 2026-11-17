/** Isolated installed-Chrome playback/layout/download tests, using its own CDP protocol. */
import fs from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
import crypto from 'node:crypto';
import {spawn} from 'node:child_process';
const ROOT=path.resolve(import.meta.dirname,'../..');
const P=path.join(ROOT,'ArrowSwarm_Marketing/05_Reels/ConceptBacklog_20261117');
const BASE='http://127.0.0.1:8200';
const URL=BASE+'/marketing/05_Reels/ConceptBacklog_20261117/index.html';
const LIB=BASE+'/marketing/Final_Deliverables/VideoLibrary/index.html';
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
const read=async p=>JSON.parse(await fs.readFile(p,'utf8'));
const assert=(x,msg)=>{if(!x)throw Error(msg)};
async function poll(fn,label,timeout=20000){const end=Date.now()+timeout;while(Date.now()<end){try{const v=await fn();if(v)return v}catch{}await sleep(100)}throw Error('Timed out: '+label)}
async function connect(url){
 const ws=new WebSocket(url);await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j});let next=0;const pending=new Map();const listeners=[];
 ws.onmessage=({data})=>{const m=JSON.parse(data);if(m.id){const p=pending.get(m.id);if(p){pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result)}}else listeners.forEach(f=>f(m))};
 return {ws,listeners,send(method,params={}){return new Promise((resolve,reject)=>{const id=++next;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}))})}};
}
const profile=await fs.mkdtemp(path.join(os.tmpdir(),'ArrowSwarm-Chrome-VideoQA-'));
const chrome=spawn('C:/Program Files/Google/Chrome/Application/chrome.exe',['--headless=new','--remote-debugging-port=0','--user-data-dir='+profile,'--no-first-run','--no-default-browser-check','--mute-audio','about:blank'],{windowsHide:true,stdio:'ignore'});
let browser,cdp;const errors=[],httpErrors=[],results=[];const qa=path.join(P,'QA');await fs.mkdir(qa,{recursive:true});
try{
 const port=await poll(async()=>{const s=await fs.readFile(path.join(profile,'DevToolsActivePort'),'utf8');return +s.split('\n')[0]},'own Chrome CDP');
 const host='http://127.0.0.1:'+port;const version=await (await fetch(host+'/json/version')).json();
 const protocol=await (await fetch(host+'/json/protocol')).json();
 for(const name of ['Runtime','Page','Network','Emulation','Browser','Input'])assert(protocol.domains.some(x=>x.domain===name),'CDP domain '+name);
 browser=await connect(version.webSocketDebuggerUrl);
 const target=await (await fetch(host+'/json/new?about:blank',{method:'PUT'})).json();cdp=await connect(target.webSocketDebuggerUrl);
 cdp.listeners.push(m=>{if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails.text);if(m.method==='Network.responseReceived'&&m.params.response.status>=400)httpErrors.push({status:m.params.response.status,url:m.params.response.url})});
 await cdp.send('Page.enable');await cdp.send('Runtime.enable');await cdp.send('Network.enable');
 async function evaluate(expression){const r=await cdp.send('Runtime.evaluate',{expression,awaitPromise:true,returnByValue:true,userGesture:true});if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));return r.result.value}
 async function viewport(width,height){await cdp.send('Emulation.setDeviceMetricsOverride',{width,height,deviceScaleFactor:1,mobile:false})}
 async function go(url){await cdp.send('Page.navigate',{url});await poll(()=>evaluate('document.readyState==="complete"&&!!document.querySelector("#cards")'),'page ready');await sleep(150)}
 async function click(expression){
  const r=await evaluate(`(()=>{const e=${expression};if(!e)throw Error('Missing element');e.scrollIntoView({block:'center',behavior:'instant'});const r=e.getBoundingClientRect();return{x:r.x+r.width/2,y:r.y+r.height/2}})()`);
  await cdp.send('Input.dispatchMouseEvent',{type:'mousePressed',button:'left',clickCount:1,...r});await cdp.send('Input.dispatchMouseEvent',{type:'mouseReleased',button:'left',clickCount:1,...r})}
 async function play(){
  await poll(()=>evaluate('(()=>{const v=document.querySelector("video");return v.readyState>=2&&v.currentTime>.2&&!v.paused})()'),'real media playback');
  return evaluate('(()=>{const v=document.querySelector("video");const r={duration:v.duration,width:v.videoWidth,height:v.videoHeight,time:v.currentTime,error:v.error?.message||null};v.pause();return r})()')}
 async function screenshot(name){await evaluate('document.querySelector("#viewer").scrollIntoView({block:"start",behavior:"instant"})');await sleep(100);const r=await cdp.send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false});await fs.writeFile(path.join(qa,name),Buffer.from(r.data,'base64'))}
 await viewport(1280,900);await go(URL);assert(await evaluate('document.querySelectorAll(".card").length')===12,'12cards');
 await evaluate('document.querySelector("video").muted=true');
 for(let i=0;i<12;i++){
  await click(`document.querySelectorAll('.card')[${i}]`);const info=await play();assert(info.width===1080&&info.height===1920&&Math.abs(info.duration-18)<.05,'dimensions/duration R'+i);
  results.push({id:'R'+String(i+1).padStart(2,'0'),realBrowserPlayback:true,...info});console.log('BROWSER',i+1)}
 await click('document.querySelector("#previous")');assert(await evaluate('document.querySelector("#position").textContent')==='11 / 12','previous');
 await click('document.querySelector("#next")');assert(await evaluate('document.querySelector("#position").textContent')==='12 / 12','next');
 await evaluate('document.querySelector("video").pause();document.querySelector("#search").value="gökkuşağı";document.querySelector("#search").dispatchEvent(new Event("input"))');assert(await evaluate('document.querySelectorAll(".card").length')===1,'search');
 await evaluate('document.querySelector("#search").value="";document.querySelector("#search").dispatchEvent(new Event("input"))');
 await click('[...document.querySelectorAll("#filters button")].find(x=>x.textContent==="Menü")');assert(await evaluate('document.querySelectorAll(".card").length')===1,'menu filter');
 await click('[...document.querySelectorAll("#filters button")].find(x=>x.textContent==="Tümü")');
 await click('document.querySelector(".card")');await play();await screenshot('desktop.png');
 for(const x of (await read(path.join(P,'manifest.json'))).items){const r=await fetch(URL.replace('index.html',`${x.id}/${x.file}`),{headers:{Range:'bytes=0-1023'}});assert(r.status===206&&(await r.arrayBuffer()).byteLength===1024,'range '+x.id)}
 const downloads=path.join(profile,'Downloads');await fs.mkdir(downloads,{recursive:true});await browser.send('Browser.setDownloadBehavior',{behavior:'allow',downloadPath:downloads});
 await click('[...document.querySelectorAll("a")].find(x=>x.textContent.includes("12 videoyu indir"))');
 const archive=path.join(downloads,'ArrowSwarm_12_Concepts_TR.zip');await poll(async()=>{try{return(await fs.stat(archive)).size>0}catch{return false}},'browser ZIP download');
 const downloadHash=crypto.createHash('sha256').update(await fs.readFile(archive)).digest('hex');assert(downloadHash===(await read(path.join(P,'BUNDLE_QA.json'))).sha256,'browser ZIP hash');
 await viewport(390,844);await go(URL);assert(await evaluate('document.documentElement.scrollWidth<=innerWidth'),'mobile overflow');
 await evaluate('document.querySelector("video").muted=true');await click('document.querySelector(".card")');const mobile=await play();await screenshot('mobile.png');
 await viewport(1280,900);await go(LIB);assert(await evaluate('document.querySelectorAll(".card").length')===39,'39 library cards');
 await click('[...document.querySelectorAll("#filters button")].find(x=>x.textContent==="Arşiv / önceki sürüm")');assert(await evaluate('document.querySelectorAll(".card").length')===5,'archive filter');
 await click('[...document.querySelectorAll("#filters button")].find(x=>x.textContent==="Savunma")');await evaluate('document.querySelector("video").muted=true');await click('document.querySelector(".card")');const old=await play();
 await click('[...document.querySelectorAll("#filters button")].find(x=>x.textContent==="Tümü")');await screenshot('library.png');
 await cdp.send('Page.navigate',{url:BASE+'/#%2Fmarketing%2F05_Reels%2FConceptBacklog_20261117%2Findex.html'});
 await poll(()=>evaluate('document.querySelector("iframe")?.contentDocument?.querySelectorAll(".card").length===12'),'hub iframe');
 assert(!errors.length&&!httpErrors.length,JSON.stringify({errors,httpErrors}));
 const report={date:'2026-11-18',passed:true,browser:version.Browser,automation:'Installed headless Chrome, isolated profile, documented native CDP; no user browser/authentication state touched',desktop:[1280,900],mobile:[390,844],allTwelveActualPlayback:results,mobilePlayback:mobile,libraryPlayback:old,galleryCardCount:12,libraryCardCount:39,archiveFilterCount:5,mobileNoHorizontalOverflow:true,range206All12:true,zipBrowserDownloadSha256Matches:true,hubIframeGallery:true,pageErrors:errors,httpErrors,subjectiveListeningClaimed:false};
 await fs.writeFile(path.join(P,'BROWSER_QA.json'),JSON.stringify(report,null,2)+'\n');console.log('PASS browser12 / mobile / library39 / ZIP / ranges / hub');
}finally{try{if(browser)await browser.send('Browser.close')}catch{}cdp?.ws.close();browser?.ws.close();chrome.kill()}
