# Desktop testing

No headset needed. With no XR device present, `DesktopRig` takes over the camera (the log says
`[TWIN] No XR device: DesktopRig on`). OpenXR logs a warning that it could not start; that is expected.

## Full run
Open `00_Boot`, Play, press any key. Vote with **1** (normal) / **2** (strange) when the panel shows.
A full run takes about five minutes. The END OF DEMO screen shows the final state and the ending name.

## Test one branch without playing from the start
* Press **F5–F12** in Play mode to jump between scenes. State is kept, so you can set it up in an
  earlier scene and jump ahead.
* Or press Play in the scene itself. Its `SceneBootstrapper` object has **Apply Debug State**: tick it and
  set `strangeCount`, `route`, `notePocketed`, `fork 3 strange` to start from that situation.
  Examples: 05_Shop with route B shows the florist; 07_LastMorning with 2 strange or the note pocketed
  shows the door and runs fork 4; with 0 strange and no note it skips the vote.
* **N** skips to the next scene, **R** restarts, **F1** hides the overlay.

## Checklist (what "works" means per scene)
| Scene | Check |
|---|---|
| 01 | alarm rings and glows; click stops it; after 2.5 s the clock flips to 11:11 and rings once; "Good morning, Sam."; door prompt turns to "Open the door" |
| 02 | mirror shows the room, never the player; reading the note starts fork 1; note goes to the bin or the camera; door unlocks |
| 03 | breakfast line, phone rings, fork 2; normal: fridge opens, bowls, milk line; strange: phone, two lines; door unlocks |
| 04 | walking past the middle of the street: ball hits from behind, rolls back uphill and stops; child stands still while in view, approaches while not; shop door |
| 05 | dressing matches the route; shopkeeper line; fork 3; normal: item follows the camera, front door; strange: panel vanishes, yellow opening, backrooms door |
| 06A | bedroom, clock 11:11, bed → scene 7 |
| 06B | yellow corridors; at the hall the stage lights switch on; one door → scene 7 |
| 07 | furniture up the walls; note in hand only if pocketed; door only if `strangeCount >= 2` or note; fork 4 only with the door; bed or door → END OF DEMO |
| 99 | strangeCount, route, notePocketed and ending name; R or button restarts |

## Automated checks
EditMode tests (Window > General > Test Runner): `GameStateTests`, `EndingResolverTests`, `SceneFlowTests`.

The scenes were also driven headlessly through the Unity CLI (`unity command script eval`) during the
build: every interaction above was triggered from script and the state read back, both routes, both
scene-6 branches, scene 7 with and without the door, and the vote timeout. Screenshots from that run
are in `docs/screenshots/`.
