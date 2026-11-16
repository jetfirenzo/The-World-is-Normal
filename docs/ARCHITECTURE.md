# Architecture

Everything lives under `Assets/TWIN/`. Namespaces follow the folders (`TWIN.Core`, `TWIN.Vote`, …).

## The one persistent object

`Resources/TWIN_Persistent.prefab` is spawned once and survives every scene load (`DontDestroyOnLoad`).
It carries:

| Component | Job |
|---|---|
| `GameManager` | singleton; owns `GameState`, the `SceneFlow` asset, the lines asset, the 4 fork assets; `GoToNextScene()` / `LoadScene()` with a fade |
| `Narrator` | subtitle queue (`Say`, `SayAs`, `WaitUntilDone`); the placeholder voice is a generated blip |
| `ForkController` | runs a fork: asks the vote provider, writes the result into `GameState`, calls the scene back |
| `KeyboardVoteProvider` | the desktop stand-in for the live audience (keys 1/2, two buttons, countdown, tally) |
| `ScreenFader` | black fade for transitions |
| `DebugOverlay` | F1 overlay, scene jump keys |
| Canvas + EventSystem | overlay UI: subtitle, prompt, crosshair, fork panel, debug text, fader |

Who spawns it: `SceneBootstrapper` (one in every scene) instantiates the prefab from `Resources` when
`GameManager.Instance` is null, so any scene can be played standalone.

## State

`GameState` (plain C# class, `Scripts/Core/GameState.cs`) is the only home of story variables:
`strangeCount`, `route`, `notePocketed`, plus `forkWasStrange[4]` / `forkAnswered[4]`.
All changes go through `ApplyFork(index, strange)`, which also applies the fork's side effects
(fork 1 strange → `notePocketed`, fork 2 → `route`). `DoorAppearsInLastMorning` is the scene 7 rule.
`EndingResolver.Resolve(state)` is the single function that names the ending.

## Scene flow

`Data/SceneFlow.asset` lists the scene names; `SceneFlow.NextAfter(current, state)` decides the
successor, including the only branch (after the shop: fork 3 normal → `06A_FrontDoor`, strange → `06B_Backrooms`).
`GameManager.GoToNextScene()` uses it. Scenes never name each other directly, except `Door.targetScene`
when a door should bypass the flow (unused in the greybox).

```
00_Boot → 01_Bedroom → 02_Bathroom → 03_LivingRoom → 04_Street → 05_Shop ─┬→ 06A_FrontDoor ─┐
                                                                         └→ 06B_Backrooms ─┴→ 07_LastMorning → 99_EndOfDemo → 00_Boot
```

## A scene

Each scene has a **director** MonoBehaviour (`Scripts/Scenes/S1_Bedroom.cs` …) holding references to
the props it needs. The director wires `Interactable.onInteract` listeners in `Start`, runs its beats as
coroutines, and asks `GameManager.Instance.forkController.RunFork(i, callback)` at its fork.
Progress gates are `Door.Lock()/Unlock()` and `Interactable.available`.

Reusable building blocks (`Scripts/Player/`):

* `Interactable` — prompt + `onInteract` UnityEvent; anything the player can use. Needs a collider.
* `Door` — an Interactable that loads the next scene when unlocked.
* `TriggerZone` — fires when the player (tag `Player`) walks in.
* `DesktopRig` — WASD + mouse look + click/E interaction, active only without an XR device.
* `LookAwareness` — frustum test used by `WeepingChild`.

Scene-specific behaviours: `MirrorNoReflection` (reflection camera that culls the `Player` layer),
`BallFromBehind`, `WeepingChild` (also the shopkeeper, with `canMove = false`).

## Player rig

`Prefabs/PlayerRig.prefab`: `CharacterController` + `XROrigin` + camera offset + `Main Camera`
(`TrackedPoseDriver` bound to the HMD centre eye) + `DesktopRig`. On desktop `DesktopRig` disables the
tracked pose driver and drives the rig itself; with a headset present it disables itself. The XR
Interaction Toolkit interactors are **not** in the rig yet; see HOW-TO-CHANGE.md for the VR step.

## Vote

`IVoteProvider` is the seam for the live audience: `StartVote(fork, onResult)` / `CancelVote()`.
`ForkController.providerBehaviour` points at whichever MonoBehaviour implements it (currently
`KeyboardVoteProvider` on the persistent prefab). `ForkDefinition` assets hold the labels and timer.

## Build pipeline

`Editor/GreyboxBuilder.cs` (+ `GreyboxScenes.cs`) generates everything from code so the greybox is
reproducible and diffable. Materials and data assets are created only when missing; prefabs and
scenes are overwritten on every run. Build Settings scene list is set from `SceneFlow.All`.
