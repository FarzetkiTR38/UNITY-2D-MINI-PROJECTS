# Localization execution plan

Prepared 13 November 2026. These priorities are operational hypotheses based on verification cost and current assets, not measured country ROI, search demand, CPI or payer value.

## Inventory and first languages

The repository contains ten localization JSON files under Assets/_Project/Data/Localization: en,tr,de,fr,it,es,pt,ru,ja,ko. Their existence does not prove complete translation quality or device coverage. Current marketing pilot captions have TR/EN variants but native HUD is English. The user rejected the initial visual direction; translation must not turn those drafts into approved release assets.

| Wave | Locales | Reason and gate |
|---|---|---|
| 0 | English and Turkish | Developer can directly review Turkish; English provides one common reference. Finish terminology, native HUD captures and release QA before upload. |
| 1 trial | Spanish, Portuguese or German, one at a time | Already represented in game data. Choose using actual acquisition/query/language evidence plus availability of a reviewer, not an assumed market ranking. |
| Later | French, Italian, Russian, Japanese, Korean and remaining Wave1 languages | Expand only after native proofreading, font/UI capture tests, support capacity and distribution/legal checks. |

Do not enable all countries/languages simply because translation files exist. Country-specific availability, device support and any export/legal requirements are release decisions. A language is not the same as a country; Portuguese requires a conscious locale decision such as pt-BR versus pt-PT, and Spanish similarly needs a locale strategy. Do not describe the whole game as supporting ten polished languages before verification.

## Shared glossary

| Internal mode/key | English | Turkish |
|---|---|---|
| Defense | Defense | Savunma |
| Shapes | Shapes | Şekiller |
| ColorShift | Color Shift | Renk Değişimi |
| lives | lives | can |
| stars | stars | yıldız |
| hint | hint | ipucu |
| rotate frame | rotate the frame | çerçeveyi döndür |

Keep the product name ArrowSwarm stable. Do not introduce Classic as a fourth mode. Explain frame rotation separately from arrow movement. Translate meaning rather than copying English keyword order. Copywriters must not add “offline”, “ad-free”, multiplayer or medical claims.

## Per-locale acceptance checklist

1. Native speaker reviews title, short/long copy and glossary; record reviewer/date rather than invent approval.
2. Re-run validate_metadata.py after every edit; text must fit the same 30/80/4000 limits.
3. Confirm every JSON key used in startup, tutorial, skills, win/lose and campaigns. Check fallback text and plurals.
4. Run native Light/Dark UI at supported device ratios. Check Turkish İ/ı/Ğ/ğ, long German text, CJK glyph coverage, truncation and input names. No fresh Unity/device tests were available in this document pass.
5. Capture screenshots and short clips with actual localized HUD and controls. Editorial captions alone are insufficient for the final localized store set.
6. Check caption line length, subtitle timing and platform-safe text placement in a real platform preview before posting.
7. Review rating/commercial/privacy statements for that locale and target distribution; ensure support contact can handle incoming messages.
8. Preview the actual localized listing and app on a device; save dated evidence. No Console preview has occurred here.

Google Play permits localized text/graphics; missing graphic translations can fall back to default assets. Automated translation is an aid, not proof of quality. Do not enable automatic rewriting of existing game translations without checking its scope. [Official localization reference](https://support.google.com/googleplay/android-developer/answer/9844778?hl=en).

## Decision inputs after launch

Track store visitors, installs and early progression by available language/country cohorts; retain unknowns when attribution is absent. Prioritize a locale when there is sustained relevant traffic and a reviewer/support path. Set an experiment cost/time cap before spending. No localization service purchase or language expansion was executed.
