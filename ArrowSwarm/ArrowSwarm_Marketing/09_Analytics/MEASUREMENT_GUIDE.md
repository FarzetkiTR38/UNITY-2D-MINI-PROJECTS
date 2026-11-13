# KPI definitions and data entry

KPI_DASHBOARD.csv is a blank measurement template, not a connected service. No real player results exist. Blank values mean NOT_MEASURED, not zero.

- D1/D7 use UTC first-open calendar cohorts. Numerator: distinct cohort users returning on day1/day7. Denominator: eligible first-open cohort old enough for that observation. Do not include immature cohorts or mix calendar-day and rolling24h definitions.
- Tutorial completion: distinct natural completers / distinct starters in a fixed window. Skip is separate. Do not treat both SetTutorialCompleted branches as the same semantic result.
- Level funnel uses mode+level+attempt. Retry is a new attempt; repeated terminal callback is not a new outcome. Early drop-off requires an observation window; an interrupted session alone is not permanent churn.
- Active time excludes loading, ads, pause and background; use monotonic timing. A proposed30min session timeout is a planning assumption, not implemented behavior.
- Store conversion must preserve the Play Console report's visitor/acquisition definition, filters and period. Video views are not store visitors.
- Platform video view definitions differ. YouTube engaged views must not be replaced by raw starts/replays. Compare within one platform, language and similar duration/window.
- Ad request/load/start/impression/reward/revenue are distinct. Deduplicate stale callbacks. Revenue units and precision need provider validation and reconciliation with reporting delays.
- Exclude Editor, preview, solver captures, internal QA and test ads. Proposed events carry schema_version/build/environment/mode, without names, raw touch coordinates, exact device fingerprints or free text.

Zero/missing denominator leaves a rate unavailable. Aggregate ratios by summing compatible numerators and denominators, not averaging percentages. Segment by version/device/mode only where the sample supports it. The CSVs record proposed definitions and blank inputs; no SDK was installed.
