# The World Is Normal — greybox

A VR narrative experience where a live audience votes at four forks. This repository holds the
**greybox**: every scene from the bedroom (1) to the last morning (7) is playable on a desktop with
no headset, with placeholder geometry, generated sound and subtitles instead of voice.

* Unity **6000.6.5f1**, Universal Render Pipeline, Input System, XR Interaction Toolkit 3.6 + OpenXR (headset support is wired but untested).
* No ending scenes yet: after fork 4 an **END OF DEMO** screen shows the final state and the ending that *would* play.
* Project settings changed from the VR template: both URP configs (`Assets/Settings/Project Configuration`) now render additional lights per pixel with a per-object limit of 8 (the template had them off, so point lights in the backrooms did nothing). Revisit this when optimising for a standalone headset.

## Play it (desktop)

1. Open the project in Unity 6000.6.5f1 (Unity Hub or `unity open "<path>"`).
2. Open `Assets/TWIN/Scenes/00_Boot.unity` (menu **Tools > TWIN > Open Boot Scene**) and press Play.
3. Press any key on the title screen.

| Input | Action |
|---|---|
| W A S D / arrows, Shift | walk, run |
| mouse | look |
| left click or E | use the thing under the crosshair (its prompt is shown) |
| 1 / 2, or the on-screen buttons | audience vote: NORMAL / STRANGE |
| Esc | free the mouse cursor; click to lock it again |
| F1 | toggle the debug overlay (state + resolved ending) |
| N / R | skip to the next scene / restart the demo |
| F5–F12 | jump to 01 … 07 (F10 = 06A, F11 = 06B) |

You can also press Play in **any** scene directly; it bootstraps itself (see `SceneBootstrapper` for a
debug start state).

## Where things are

```
Assets/TWIN/
  Scenes/            00_Boot … 99_EndOfDemo (generated, see below)
  Scripts/           all gameplay code (assembly TWIN.Runtime)
  Editor/            GreyboxBuilder: generates materials, prefabs, data and scenes from code
  Data/              SceneFlow, NarratorLines, Forks/* — edit text and order here, no code needed
  Prefabs/           PlayerRig, Child
  Resources/         TWIN_Persistent (GameManager + UI, spawned once, survives scene loads)
  Tests/EditMode/    unit tests for state, ending rules and scene order
docs/
  STORY.md           the story brief (source of truth)
  ARCHITECTURE.md    how the pieces fit together
  HOW-TO-CHANGE.md   recipes: change a line, a fork, a scene, plug in the real audience vote
  DESKTOP-TESTING.md how to test every branch without playing from the start
  ENDINGS-TODO.md    the missing ending rules
```

## Regenerating the scenes

The scenes are **generated** by `Tools > TWIN > Build All Greybox Scenes`. Running it again overwrites
all ten `.unity` files (data assets and materials are kept). If you hand-edit a scene in the Editor,
either stop regenerating or put your change into `Assets/TWIN/Editor/GreyboxScenes.cs` so it survives.

## Tests

Window > General > Test Runner > EditMode, or from a terminal:

```bash
unity test "D:\git\The World is Normal" --mode EditMode
```
