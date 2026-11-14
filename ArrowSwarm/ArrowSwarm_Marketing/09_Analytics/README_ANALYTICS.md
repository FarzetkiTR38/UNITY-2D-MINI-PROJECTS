# Analytics files and exact next action

This phase produced a source audit, proposed18-event plan,19-KPI manual-input dashboard,6 experiments, three illustrative budget cases and privacy release questions. It did not install analytics or launch a campaign.

1. Read ANALYTICS_AUDIT.md and PRIVACY_RELEASE_QUESTIONS.md.
2. Resolve actual build/SDK/network/provider/consent evidence before approving a collector.
3. For existing report data, copy KPI_DASHBOARD.csv and enter matching period,segment,numerator and denominator. Keep unavailable data blank. Use MEASUREMENT_GUIDE.md definitions.
4. Optional local calculation: `python calculate_kpis.py --input your_copy.csv --output your_report.json`. It has no network code; missing inputs stay unavailable. Ratios are fractions, so0.25 means25% for percentage metrics; active-session duration remains seconds. No arbitrary target or estimate is generated.
5. New collection/SDK requires a separate concrete implementation decision; this package is not that implementation.

CSV data was authored/round-tripped with the bundled artifact workbook library, then exported as CSV per task. `growth_tables_source.json` is the structured source for all five tables, `table_authoring_qa.json` records the authoring check and `GROWTH_PACKAGE_QA.json` records current row/status checks. No XLSX was requested or substituted.

08_Social_Media contains the30-day relative calendar, platform strategy, captions/profiles/community drafts and five public creator/outlet candidates. None is published. Qualified micro-creator research remains open; large outlets are not relabelled micro.
