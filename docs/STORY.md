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

## Endings: NOT now

Do not build the ending scenes. After fork 4, show a placeholder screen: "END OF DEMO", plus the
final `strangeCount`, `route`, `notePocketed` and the name of the ending that WOULD play, computed by
one function `EndingResolver.Resolve(GameState)` with these rules:

* `strangeCount == 4` → "System error" (overrides everything else)
* *(the brief was cut off here — see ENDINGS-TODO.md)*
