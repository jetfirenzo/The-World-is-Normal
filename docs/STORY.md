# The World Is Normal — story (source of truth)

A player wakes up as "Sam" in a world that pretends to be normal. A live audience votes at 4 forks
with two buttons. Each fork has a NORMAL option and a STRANGE option. The game keeps:
`strangeCount` (0–4), `route` (A or B), `notePocketed` (bool).

**Scene 1, Bedroom.** The alarm rings, the player clicks it off. Silence. The clock flips back to
11:11 and rings once more by itself. Narrator: "Good morning, Sam."

**Scene 2, Bathroom.** A note on the sink reads "Your name isn't Sam." The mirror shows the bathroom
but NO reflection of the player.
FORK 1: throw the note in the bin (normal) / put it in your pocket (strange, `notePocketed = true`).

**Scene 3, Living room.** A table set for one, a bowl of cornflakes. Narrator: "Breakfast is the
most important meal of the day."
FORK 2: look for milk (normal, route A) / answer the ringing phone (strange, route B).
Route A: the fridge holds only more bowls of cornflakes. Narrator: "We're out of milk. Let's go to the shop."
Route B: the voice on the phone: "Your mom died this morning." Narrator, cheerfully: "How sad. Let's get her some flowers."

**Scene 4, Street.** Identical houses, every door number 9. A ball hits the player in the back;
nobody is there. The ball rolls back to him, uphill. A child stands still at the end of the street
and only moves closer when the player isn't looking.

**Scene 5, Shop.** ONE room with two dressings.
Route A, supermarket: every shelf full of milk, every carton shows a missing-person photo of Sam: "Have you seen this man?"
Route B, florist: every bouquet carries the card "In loving memory of your mom."
The shopkeeper has the SAME face as the child. "That's free. Everything is free today."
FORK 3: take it and go home (normal) / look behind the counter (strange).

**Scene 6, The way home.**
Normal: the front door opens straight into the bedroom. 11:11.
Strange: behind the counter, long yellow backrooms corridors. At the end, the street seen from
behind: the houses are flat wooden facades held up by beams, a stage set. One door leads back to the bedroom.

**Scene 7, Last morning.** The bedroom again, now wrong: the furniture has slid halfway up the walls.
The alarm rings. If `notePocketed`, the note is in the player's hand and now reads "Don't go back to
sleep." If `strangeCount >= 2` OR `notePocketed`, a door has appeared in the wall.
FORK 4 (only if the door exists): go back to bed (normal) / open the door (strange). No door = no vote, Sam goes back to bed.

## Endings

After fork 4 the resolved ending plays (the END OF DEMO screen is now a debug fallback).
`EndingResolver.Resolve(GameState)` picks one:

* `strangeCount == 4` → SYSTEM ERROR (overrides all)
* door opened, `strangeCount == 3` → WAKE UP
* door opened, `strangeCount == 2` → THE FUNERAL
* back to bed, `strangeCount` 2–3 → THE NOTE
* back to bed, `strangeCount` 0–1 → GOOD MORNING, SAM (also when no door appeared)

Every ending finishes with "Press R to start a new run" (later: the operator presses a button).

**1. GOOD MORNING, SAM** (bad ending, the loop). The player lies down. The screen slowly fades to black,
as if eyes close. Narrator, warm: "Good boy, Sam." Two seconds of black and silence. Then the alarm
rings and the clock shows 11:11. Fade back into the normal Bedroom from scene 1. Restart prompt over it.
Haptics: alarm click on the hands at the end.

**2. THE NOTE** (twist, the loop explained). The player lies down. A short fade. The player is suddenly
standing next to the bed. Someone else is waking up in the bed (the shared NPC, under the duvet, face
turned away). The player's hand moves on its own to the bedside table, picks up a pen and writes the
note: "Your name isn't Sam." The text appears letter by letter on PROP_Note. The figure in the bed starts
to sit up. Cut to black. Haptics: a slow pulse on the hands while the hand writes.

**3. THE FUNERAL** (dark). The door in the wall opens into a small funeral room: rows of chairs, flowers,
soft organ hum. An open coffin with Sam in it, nameplate "SAM". Next to it his mother: the shared NPC,
same face as the shopkeeper. She turns slowly toward the player and smiles: "Morning, Sam." Hold one
beat, then black. Haptics: heartbeat that gets faster as the mother turns.

**4. WAKE UP** (good ending). The door opens onto pure white. Sound fades to total silence over
3 seconds. Everything fades to white. Text: "Take off the headset." Later: switch the Quest 3 to
passthrough (`WakeUpController.EnablePassthrough()`, not implemented yet).
Haptics: *the brief was cut off here*; implemented as "all haptics stop".

**5. SYSTEM ERROR.** *Not described in the brief yet.* Placeholder: glitch, "SYSTEM ERROR", prompt.
