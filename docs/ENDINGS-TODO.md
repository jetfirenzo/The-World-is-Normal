# Endings — TODO

The brief listed the ending rules but was cut off after the first one. Only that rule is implemented.

## What exists now

`Assets/TWIN/Scripts/Core/EndingResolver.cs`:

```csharp
public static string Resolve(GameState state)
{
    if (state.strangeCount == 4) return "System error";   // rule 1, overrides everything
    return "TODO: ending rules not yet specified";        // placeholder
}
```

The END OF DEMO screen (scene `99_EndOfDemo`) and the F1 debug overlay both print the result of this
function, so once the rules are in, the right ending name shows up everywhere automatically.

## How to add the remaining rules

1. Add each rule to `Resolve`, **in priority order** (the first matching rule wins). Available inputs:
   `state.strangeCount` (0–4), `state.route` (`Route.A` / `Route.B`), `state.notePocketed`,
   `state.forkWasStrange[0..3]` (per fork), `state.forkAnswered[0..3]`.
2. Give each ending a `public const string` next to `SystemError`.
3. Extend `Assets/TWIN/Tests/EditMode/EndingResolverTests.cs`: one test per rule, plus the test
   `LessThanFourStrange_IsStillUnknown` must be replaced (it currently asserts the placeholder).
4. Run the EditMode tests.

No scene needs to change. Ending *scenes* are intentionally not built yet; when they are, load them
from `GameManager.GoToNextScene` after `lastMorning` instead of `endOfDemo` (see `SceneFlow.NextAfter`).
