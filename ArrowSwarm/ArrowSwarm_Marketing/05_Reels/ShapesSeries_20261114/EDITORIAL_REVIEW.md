# Shapes series: independent editorial inspection

Review scope: final encoded stills and contact sheets were inspected visually through the image tool; source geometry, event logs, edit plans and technical QA were read separately. This is a sampled visual review, not a claim of watching every frame or subjectively listening to the narration/music. The parent owns native Unity capture and state-restoration verification.

## Inspected evidence

| Episode | Evidence actually viewed | Observation |
|---|---|---|
|12 / kaplumbağa|Final `cover.jpg` (6.5s); encoded contact sheet; final MP4 extracts at14.0s and34.5s|Three quiz badges connect to their intended arrow heads. At14.0s A has fully exited, counter1/45, three lives remain. At34.5s the native WinPanel shows three gold stars and45/45; dimmed TopHUD and BottomHUD remain visible; success caption fits in the gap above BottomHUD.|
|16 / ördek|Encoded contact sheet; final MP4 extracts at14.0s and33.5s|At14.0s exactly one arrow has left (1/42). At33.5s native three-star victory and42/42 are visible; both HUDs remain. Success caption does not cover the popup or BottomHUD.|
|21 / ahtapot|Pre-encode6.5s quiz still; encoded contact sheet; final MP4 extracts at14.0s and33.5s|A/B/C connector geometry is readable. At14.0s counter1/44. At33.5s native three-star victory and44/44, with both HUDs retained.|
|5 / roket|Revised pre-encode6.5s quiz still; encoded contact sheet; final MP4 extracts at14.0s and33.0s|Compact single-line caption stays above the tall board. Gutter badges leave the actual arrow heads visible. At14.0s counter1/40; at33.0s native three-star victory and40/40. Both HUDs remain visible.|
|29 / flamingo|Revised pre-encode6.5s quiz still; final encoded MP4 extracts at6.5s,14.0s and33.5s|At6.5s the displaced A badge leaves its right-edge arrow head visible, with connector attached; B/C are readable. At14.0s A has fully exited and counter1/40; at33.5s native three-star victory and40/40. Both HUDs remain visible and captions clear them.|
|39 / ayçiçeği|Pre-encode6.5s quiz still; final encoded MP4 extracts at6.5s,14.0s and33.5s|At6.5s the compact caption and right-gutter badges leave the tall board and intended heads readable. At14.0s A has fully exited and counter1/44; at33.5s native three-star victory and44/44. Both HUDs remain visible and captions clear them.|

QA extracts use ignored `still_qa_*.jpg` files in each episode folder; they are review aids, not additional deliverables.

## Concrete issues found and corrected

- L05 originally used the standard large upper caption and circular countdown over the rocket nose, and its bottom A badge covered its own path. The renderer now uses a compact single-line upper caption and side-gutter badges for tall layouts. The updated6.5s still and final14.0s extract show the obstruction removed.
- L29 initially placed the right-gutter A badge at the same height as the head at approximately(1009,620), concealing that head. The revised badge sits lower, connected back to the unobstructed head; the updated6.5s still was inspected.
- Idle-hold metadata now distinguishes source idle duration from output idle duration, preventing a future editorial hold from being mislabeled.

## Geometry and timing checks

The independent local analysis mirrors production integer head-ray collision and ascending-index saturation. The three selected arrows are proven separately from other initially legal arrows: A is legal, B/C are blocked. This does not claim that A is the only legal move anywhere on the board. Native geometry exported by production was checked against the L21 and L05 draft choice IDs and states; the renderer additionally asserts production-removability flags, exact solution/event ordering, level asset hashes, shot counts and three lives for each source.

Renderer timing was read: the first shot runs at normal speed and is followed by an idle interval; remaining-action narration completes before the second shot. Only the marked middle section is1.5x. The last-arrow section and native victory return to normal speed. Common outro onset follows the complete success clip with a positive buffer. L12 specifically: native victory29.422s; congratulations30.333–35.163s; outro starts35.367s.

L12 final decode log reports1250frames without decode errors. Its final technical loudness report measures−16.64LUFS integrated and−1.49dBTP. These measurements do not establish perceived sound quality. The shared outro retains the approved phonetic CTA voice and mock store link; no public store availability is claimed.

## Limits

- No full subjective audio audition was performed by this independent reviewer.
- Per-frame HUD pixel guards and native completion/state restoration are asserted by the capture/render pipeline and documented separately; this reviewer sampled the actual encoded images listed above.
- MP4 pointer/button interaction is editorial animation, not a clickable app-store link in the video.
- Final creative approval remains with the user.
