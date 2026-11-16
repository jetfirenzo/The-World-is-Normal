# How to change things

Recipes for the changes a teammate is most likely to make. None of them needs permission; the code
is written to be edited.

## Change a narrator / character line
Edit `Assets/TWIN/Data/NarratorLines.asset` in the Inspector. Every line in the demo is a field there.
Add a new field to `Scripts/Narration/NarratorLines.cs` if you need a new line, then use it from a
scene director with `gm.narrator.Say(gm.lines.myLine)` or `SayAs("Speaker", …)`.

## Change what a fork says, how long the vote lasts, who wins a tie
Edit the four assets in `Assets/TWIN/Data/Forks/`. `voteSeconds` is the countdown,
`strangeWinsOnTimeout` decides a 0–0 (or tied) vote. `KeyboardVoteProvider.endOnFirstVote` (on the
`TWIN_Persistent` prefab) ends the vote on the first key press; turn it off to let the timer count real votes.

## Plug in the real audience vote
1. Write a MonoBehaviour that implements `TWIN.Vote.IVoteProvider` (`StartVote(fork, onResult)` must
   call `onResult(strangeWon)` exactly once; `CancelVote()` must stop it).
   `KeyboardVoteProvider` is a complete example; you can also keep it and call its `CastVote(bool)` from
   your network code for a quick first version.
2. Add your component to `Resources/TWIN_Persistent.prefab` and drag it into
   `ForkController.providerBehaviour`.
Nothing else changes: scenes only talk to `ForkController.RunFork`.

## Change the ending rules
See `docs/ENDINGS-TODO.md`. One function, one test file.

## Change the order of scenes or rename one
Edit `Assets/TWIN/Data/SceneFlow.asset` (names must match the `.unity` files and the Build Settings).
The branch logic lives in `SceneFlow.NextAfter`.

## Change the layout of a scene
Two options:
* **Quick:** open the scene, move things, save. Do **not** run *Build All Greybox Scenes* afterwards,
  it would overwrite your edit.
* **Durable:** change the numbers in `Assets/TWIN/Editor/GreyboxScenes.cs` (`Build_04_Street` etc.,
  positions are metres) and run **Tools > TWIN > Build All Greybox Scenes**. Helpers: `Box`, `Prim`,
  `Room`, `Label3D`, `DoorObj`, `PointLight`, `Spawn`.

## Add an interaction
1. Give the object a collider and an `Interactable` (set `prompt`).
2. In the scene director, `myInteractable.onInteract.AddListener(() => …)` in `Start`, or wire it in the
   Inspector. Use `available = false` until the player is allowed to use it.

## Add a scene
1. Add a name field to `SceneFlow` and put it into `All` and `NextAfter`.
2. Write `SX_MyScene.cs` (a director) and a `Build_XX_MyScene()` in `GreyboxScenes.cs` (copy an
   existing one; always call `NewScene()`, `Spawn()` and `Save()`).
3. Run the builder; the scene is added to Build Settings automatically.

## Replace the greybox props with art
Props are plain primitives with materials from `Assets/TWIN/Materials`. Swap the mesh, keep the
`Interactable` / `Door` / collider on the same GameObject (or a parent), and keep the `Player` tag and
layer on the rig.

## Headset (VR) step
The rig has an `XROrigin` and an HMD-tracked camera, OpenXR is the Standalone loader, so a headset
should already drive the camera. To interact in VR, add XRI interactors from
`Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/Interactors` under the rig and have
their Select event call `Interactable.Interact()` on the hit object, and replace the Screen Space
Overlay canvas on `TWIN_Persistent` with a world-space one (overlay canvases do not render in VR).

## Audio
`Scripts/Util/ToneGenerator.cs` makes the alarm, ring and blip clips in memory. To use real audio,
assign clips to the `AudioSource`s in the scene directors (`alarmAudio`, `phoneAudio`) and to
`Narrator.voice`, and delete the `ToneGenerator` calls.
