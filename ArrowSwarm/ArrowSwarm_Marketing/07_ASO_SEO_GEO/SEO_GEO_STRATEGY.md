# ArrowSwarm SEO and GEO implementation brief

Prepared 13 November 2026. Root's website output is a **local, unpublished prelaunch draft**. No verified public domain, live Play URL, support address or publication date is supplied. The old icon/Reels direction remains rejected and is not the permanent brand.

## Page content contract

Use a readable main page in TR and EN, with visible text explaining ArrowSwarm and the three modes. Give Shapes, Color Shift and Defense their own sections or substantive mode pages, with links back to the game overview. Show genuine gameplay and explain whether the clip is automated/edited and which build/mode it represents. Do not call a draft “available now” or show a fake Play Store download button. A prelaunch status and a “watch gameplay” action are appropriate while availability is unknown.

Content to include:

- What the player does: choose arrow exits; Color Shift rotates the outer frame while arrows stay fixed; Defense adds moving enemies.
- A brief source-backed mode description and an actual native screenshot/clip.
- FAQ about the three modes, lives/progress and current availability. Offline, pricing, IAP and supported-device answers stay pending until release evidence exists.
- A press-kit link to actual small assets and a factual studio description. Public support/contact details remain unset until verified by the owner; no fake email or newsletter form.
- A clear prelaunch/revision notice. Rejected visuals may appear only as explicitly labelled draft material, preferably keep them in the private review page rather than the public-facing landing design.

## Technical SEO checklist

Draft titles: EN “ArrowSwarm — Arrow Puzzles in Three Modes”; TR “ArrowSwarm — Üç Modlu Ok Bulmacası”. Suggested descriptions are in website_copy.json. Put important text in HTML, keep a single clear h1, descriptive links and meaningful screenshot alt text. A CSS-light responsive page with video preload=metadata avoids unnecessary initial downloads. Reserve image dimensions to limit layout shifts; provide visible keyboard focus, labels and sufficient contrast. Test on real mobile viewports before publication.

During local review, use noindex intentionally and do not invent canonical/hreflang/og:url hostnames. Before an approved deployment, choose the real HTTPS host, replace paths with actual public URLs, set self-canonical URLs and real language alternates, create a matching sitemap and review robots/CDN access. Remove the draft noindex only when the owner approves public indexing. A robots exclusion is not a privacy control, and a local file cannot be counted as an indexed site. Verify the published host in Search Console and inspect live URLs; don't claim successful crawling/indexing now. [SEO Starter Guide](https://developers.google.com/search/docs/fundamentals/seo-starter-guide).

## Structured data without fabricated evidence

A JSON-LD syntax check is different from a Google rich-result eligibility check. A factual VideoGame/MobileApplication entity can describe the name, Android platform and three modes, with only information also visible on the page. Use publisher FarzetkiGames as the project/studio name, not an unverified legal corporate registration. Omit unsupported offers, price, ratings, reviews, release dates, download counts and sameAs links.

Google's software-app rich result has requirements including a genuine offer and a rating/review. They cannot be fabricated to make a validator pass; the current prelaunch page must **not** claim rich-result eligibility. Revisit with real release data and use the Rich Results Test after deployment. [Official software-app reference](https://developers.google.com/search/docs/appearance/structured-data/software-app).

## GEO: a reliable product record

Google's current AI-features guidance retains SEO fundamentals: useful first-hand content, crawlable pages and visible facts supported by relevant media. There is no special AI schema or required AI text file that guarantees inclusion. This project will not promise visibility in ChatGPT, AI Mode or AI Overviews. [AI features guidance](https://developers.google.com/search/docs/appearance/ai-features).

Maintain a compact facts page: studio/game name, three modes, development status, verified platform and dated source clips. Record changes when the game changes. Answer specific player questions with original explanations from the developer, rather than hundreds of near-duplicate keyword pages. Link only actual official profiles. Outreach to legitimate communities/press must be human-relevant and explicitly authorized; no fake reviews, backlink schemes, purchased mentions or automated messages.

## Measurement and next deployment action

The local draft has zero measured search impressions, clicks or AI referrals. After approved deployment, monitor Search Console queries/pages/countries/device groups and use server/referral records only with appropriate privacy controls. Keep brand/non-brand traffic separate. Review crawl/indexing and page experience before changing copy for imagined ranking effects. A simple UTM plan can link published social posts to the real landing page once both exist; it is not a substitute for install attribution. No tracking SDK, analytics beacon, cookies or account change was added by these documents.

Remaining root-side deliverables: actual landing files, mobile/accessibility checks, public-domain/Play/contact verification and an approved deployment. Final visuals remain a separate revision task. All planned filenames/URLs here are implementation directions, not evidence that those operations happened.
