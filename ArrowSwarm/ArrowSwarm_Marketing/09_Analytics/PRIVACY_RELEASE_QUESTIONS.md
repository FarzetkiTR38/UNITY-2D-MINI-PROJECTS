# Privacy release questions

13 November2026. Engineering checklist, not legal certification or final privacy policy. No Data safety form was submitted and no collection change was made.

1. Who is the controller/studio contact? What is the real privacy URL, target age, countries and Families scope?
2. Where do UGS authentication IDs and leaderboard name/country/level/stars go, who can see them and how long are they retained? A name may contain personal information; do not copy it into custom analytics.
3. What actual SDK/mediation versions, production/test ad settings and network transfers are in the signed release binary?
4. Where is the real privacy/UMP flow? Verify consent-information updates, required privacy options and ad-request gating on applicable devices/regions. Not finding calls in source does not prove a violation.
5. For each field: purpose, necessity, lawful basis, notice and any required consent? GDPR Art5/6 and KVKK requirements need a proper applicability assessment; neither anonymous labels nor blanket-consent assumptions settle this.
6. Minimize custom events. No default raw touch paths, IP/device fingerprint, advertising ID, name, profile country, free text or screenshots. Prefer aggregate counts and mode/level context.
7. How can users withdraw choices/request access/deletion? Local save deletion does not automatically delete UGS/AdMob/other service records. Check each service separately.
8. What retention and access-control policy is documented? A proposed90day raw-event cap is only an assumption; do not collect before deciding the actual purpose/service/legal requirements.
9. Does Play Data safety/privacy policy include third-party SDK behavior? Compare release manifest/network before and after choices; do not infer no-collection from missing custom analytics.
10. New SDK/collection requires a concrete implementation/approval step; none was installed here.

Sources: [GDPR](https://eur-lex.europa.eu/eli/reg/2016/679/oj/eng/), [KVKK mobile recommendations](https://www.kvkk.gov.tr/Icerik/7751/Mobil-Uygulamalarda-Mahremiyetin-Korunmasina-Yonelik-Tavsiyeler) (search text accessible; full-page retry timed out), [Play User Data](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en), [Data safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en), [AdMob UMP](https://developers.google.com/admob/unity/privacy), [Unity Analytics privacy](https://docs.unity.com/en-us/analytics/privacy-and-consent/manage-data-privacy).
