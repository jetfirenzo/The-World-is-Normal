# Endings — status and TODO

## How endings work
* `EndingResolver.Resolve(GameState)` returns the `Ending` enum, rules in priority order:
  * `strangeCount == 4` → `SystemError` (overrides all)
  * door opened, `strangeCount == 3` → `WakeUp`
  * door opened, `strangeCount == 2` → `TheFuneral`
  * back to bed, `strangeCount` 2–3 → `TheNote`
  * back to bed, `strangeCount` 0–1 → `GoodMorningSam` (also when no door appeared)

  "Door opened" is `GameState.DoorOpened` (fork 4 answered STRANGE).
* `SceneFlow.EndingScene(ending)` maps each value to its scene: `End_GoodMorningSam`, `End_TheNote`,
  `End_TheFuneral`, `End_WakeUp`, `End_SystemError`.
* After fork 4, `GameManager.GoToNextScene()` loads that scene **additively** on top of `07_LastMorning`.
  The ending reaches the last morning's bed, door and wall through `LastMorningBridge`.
  Tick `useEndOfDemoFallback` on the `TWIN_Persistent` prefab to get the old END OF DEMO screen instead.
* Each ending has a `*Controller` (`Assets/TWIN/Scripts/Endings/`) and a Timeline
  (`Assets/TWIN/Data/Endings/End_*.playable`):
  * the **Beats** signal track calls the controller's methods (lie down, fade, open door, say line …);
  * **Animation tracks** move props (the hand, the figure sitting up, the mother turning). Their clips are
    in `Data/Endings/Clips/`.
  Retime an ending by dragging markers and clips in the Timeline window. No code change needed.
* All text and voice lines are in `Data/NarratorLines.asset` (section "Endings").
* Every ending ends with "Press R to start a new run". R calls `GameManager.RestartDemo()`:
  `GameState.Reset()`, audio volume, fader and haptics back to normal, scene 1.
* Haptics go through `HapticsService` (on `TWIN_Persistent`). On desktop they only log, and the last event
  shows in the F1 overlay.

## Verified on desktop (2026-11-18)
All five endings were played from `07_LastMorning` through the Editor:

| Ending | Reached with | Checked |
|---|---|---|
| Good morning, Sam | 0 strange, no door, bed | lie down, fade, "Good boy, Sam.", 2 s black, alarm + 11:11 + click, normal bedroom, prompt |
| The note | 2 strange, back to bed | stand next to the bed, hand moves to the note (slow pulse), letters appear, figure sits up, cut to black, prompt |
| The funeral | note pocketed, door opened (2 strange) | door opens, organ, walk to the coffin "SAM", mother turns (heartbeat 60→140 bpm), "Morning, Sam.", black, prompt |
| Wake up | 3 strange, door opened | door onto white, haptics stop, fade to white and silence over 3 s, "Take off the headset.", passthrough hook logs, prompt |
| System error | 4 strange | placeholder glitch, "SYSTEM ERROR", prompt |

R after an ending returns to scene 1 with a clean state, full volume and a clear fader.

## Still open
1. **System error has no script.** The brief does not describe it. `SystemErrorController` is a
   placeholder (black glitch with random haptic pulses, "SYSTEM ERROR", prompt).
2. **Wake up haptics.** The brief was cut off at "Haptics: all haptics". Implemented as "all haptics stop"
   (`StopHaptics` at the start of the Timeline). Change the Beats track if something else was meant.
3. **Passthrough.** `WakeUpController.EnablePassthrough()` is an empty hook; its comment lists what the
   Quest 3 needs. The Timeline already calls it at the right moment.
4. **Chest heartbeat.** Quest controllers have no chest motor, so the funeral heartbeat plays on both hands.
5. **Not tested on a headset.** Haptics, the screen-space UI and the scripted player moves need a VR pass.
