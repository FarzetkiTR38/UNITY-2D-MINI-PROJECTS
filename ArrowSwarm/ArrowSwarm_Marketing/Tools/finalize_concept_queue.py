"""Resolve each original queue row to an actual delivered film and native source."""
import csv
import build_concept_backlog as c
import package_concept_backlog as p
b=c.b
def write_csv(path,fields,rows):
    with path.open('w',newline='',encoding='utf-8') as f:
        writer=csv.DictWriter(f,fieldnames=fields);writer.writeheader();writer.writerows(rows)
def main():
    manifest=b.m.read(c.PACKAGE/'manifest.json');assert manifest['complete'] and b.m.read(c.PACKAGE/'ENCODED_QA.json')['passed']
    c.prepare_sources();mapping={'D-tutorial':'tutorial','D-collision':'wrong','D-rainbow':'freeze','S-heart':'heart','S-hard':'advanced','S-error':'fail','C-match':'match','C-rotate':'color','C-budget':'budget','Menu-modes':'menu'}
    path=b.RAW/'CAPTURE_QUEUE.csv';rows=list(csv.DictReader(path.open(encoding='utf-8-sig')));fields=list(rows[0])
    for f in ('sourceDuration_s','evidence'):
        if f not in fields:fields.append(f)
    for row in rows:
        ident=row['capture_id']
        if ident not in mapping and ident!='D-winlose':continue
        keys=['defense','wrong'] if ident=='D-winlose' else [mapping[ident]]
        sources=[c.SOURCES[k] for k in keys]
        row['source']=' | '.join(s.relative_to(b.RAW).as_posix() for s in sources)
        row['sourceDuration_s']=' + '.join(f'{b.m.read(s/"provenance.json")["frames"]/30:g}' for s in sources)
        row['status']='RECORDED_VERIFIED_2026_11_18';row['dependency']='none; native sources verified'
        row['evidence']='ArrowSwarm_Marketing/05_Reels/ConceptBacklog_20261117/ENCODED_QA.json'
        if ident=='D-rainbow':row['level']='10';row['scenario']='Original planned101 explicitly substituted by genuine verified Defense10 final Rainbow hit and natural win'
        if ident=='D-winlose':row['scenario']='Two explicitly separate genuine Defense3 outcomes; win and wrong-shot loss; not a single retry recording'
    write_csv(path,fields,rows)
    path=b.m.MEDIA/'05_Reels/RENDER_QUEUE.csv';rows=list(csv.DictReader(path.open(encoding='utf-8-sig')));fields=list(rows[0])
    for f in ('output','qa','actual_source'):
        if f not in fields:fields.append(f)
    for row in rows:
        ident=row['job_id'];item=next(x for x in manifest['items'] if x['id']==ident)
        row['status']='RENDERED_QA_PASSED_REVIEW_PENDING';row['reason']=p.NOTES[ident]
        row['output']=f'ArrowSwarm_Marketing/05_Reels/ConceptBacklog_20261117/{ident}/{item["file"]}'
        row['qa']='ArrowSwarm_Marketing/05_Reels/ConceptBacklog_20261117/ENCODED_QA.json'
        row['actual_source']=' | '.join(dict.fromkeys(c.SOURCES[x['key']].relative_to(c.ROOT).as_posix() for x in item['timeline']))
    write_csv(path,fields,rows)
    assert len(rows)==12 and all(r['status']=='RENDERED_QA_PASSED_REVIEW_PENDING' for r in rows)
    print('Resolved all12render concepts / all native capture dependencies; creative review pending')
if __name__=='__main__':main()
