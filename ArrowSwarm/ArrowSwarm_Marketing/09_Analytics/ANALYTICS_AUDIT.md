# Analytics audit — read-only source inspection

13 November 2026. No game code, SDK, preferences, scene or collection behavior changed. No player telemetry, Play Console/AdMob/UGS dashboard access, device/network test or live metrics were available.

| Finding | Source under `Assets/_Project/Scripts` | Meaning and limit |
|---|---|---|
| Defense lifecycle events | `Core/GameManager.cs:47,53,56,59,62,65`; win159/loss250 | Local state/win/loss/fire/error events exist; this does not prove remote analytics delivery. |
| Defense ready/loading | `Core/LevelManager.cs:38,41,44,140,192,193` | Start timing after ready, not at loading. |
| Puzzle broad notification | `Puzzle/PuzzleSession.cs:26,43,63,86` | OnChanged repeats for many mutations. Mode, level and AttemptVersion plus state transitions are needed to prevent duplicates. |
| Puzzle outcomes | `Puzzle/PuzzleSession.Actions.cs:28,45–60,98–110` | Wrong shot / wall mismatch and terminal state are distinguishable. Final collision return resolves before outcome. |
| Tutorial flag | `Tutorial/TutorialManager.cs:276,290`; `Data/DataManager.cs:152–157` | Natural completion and alternate completion/skip paths need semantic separation. A bool does not prove a funnel event. |
| Local saves/profile | `Data/DataManager.cs:47–65`; `Data/PlayerData.cs:28,52–64` | PlayerPrefs JSON includes last login day, name, country, ID and tutorial flag. One device's login date cannot produce cohort D1/D7 retention. |
| UGS auth/leaderboard | `Data/UnityCloudService.cs:26–34,47–83,129`; Packages/manifest.json | Anonymous authentication and score metadata(name,country,level,stars) are present. This is not general product analytics or a full cloud-save proof. |
| Ads lifecycle | `Ads/AdManager.cs:69,98,122`; `Ads/AdManager.Requests.cs:16–33`; `Ads/GoogleMobileAdsService.Rewarded.cs:29–77` | Requests/results/rewards exist. A request is not an impression or revenue. |
| Ad pacing defaults | `Ads/AdManager.cs:16–17,122–138` | Serialized defaults90s cooldown/minlevel4; actual scene Inspector values need verification. |

## What was not found

Packages/manifest.json has no `com.unity.services.analytics`. The built-in `com.unity.modules.unityanalytics` module is present; that alone does not prove configured analytics or authorized data collection.

Scoped searches in `_Project/Scripts` and `Packages` found no `AnalyticsService`, `CustomEvent`, `Analytics.CustomEvent`, `UnityEngine.Analytics` or `FirebaseAnalytics` call. Third-party SDK binary/dashboard behavior is not ruled out.

Scoped `_Project/Scripts` searches found no `ConsentInformation`, `ConsentForm`, `CanRequestAds`, `PrivacyOptions`, `OnAdPaid` or `OnAdImpressionRecorded`. This is not a legal noncompliance verdict. Actual providers, mediation configuration, release binary and network/permission behavior must be checked.

No persistent cohort pipeline, session-duration collector, attribution, tutorial funnel, early-level abandonment or revenue measurement was verified. Do not present leaderboard scores as retention.

## Proposed architecture — not implemented

One event adapter following the existing C# event architecture; explicit mode/attempt context and one queue/flush owner. Filter Editor, preview, solver captures and QA devices out of production cohorts. Puzzle requires semantic events or OnChanged snapshot differences; Defense OnLevelWon is not a puzzle outcome signal.

First no-collection step: voluntary observed playtests plus actual existing Play Console/AdMob reports when access exists. Before new telemetry, decide purposes, lawful basis/notice/consent, minimal fields, retention/deletion and SDK inventory. EVENT_PLAN.csv is a proposal; no collector/sink or new SDK was added.

P0: verify actual ad/UGS data flow and privacy choices on release Android. P1: natural tutorial completion/skip, level-start/end, mode/attempt and active duration. P2: authorized attribution and impression/revenue callbacks. See PRIVACY_RELEASE_QUESTIONS.md and SOURCES_AND_LIMITS.md.
