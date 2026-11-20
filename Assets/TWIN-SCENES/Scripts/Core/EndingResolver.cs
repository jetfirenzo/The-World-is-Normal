namespace TWIN.Core
{
    /// <summary>The five endings. Each maps to its own additive scene (see SceneFlow.EndingScene).</summary>
    public enum Ending
    {
        GoodMorningSam, // bad ending, the loop
        TheNote,        // twist, the loop explained
        TheFuneral,     // dark
        WakeUp,         // good ending
        SystemError     // all four forks strange
    }

    /// <summary>
    /// Decides which ending plays after fork 4. One function, rules in priority order:
    ///   strangeCount == 4                      -> SystemError (overrides all)
    ///   door opened, strangeCount == 3         -> WakeUp
    ///   door opened, strangeCount == 2         -> TheFuneral
    ///   back to bed, strangeCount 2-3          -> TheNote
    ///   back to bed, strangeCount 0-1          -> GoodMorningSam (also when no door appeared)
    /// "Door opened" = fork 4 answered STRANGE. Opening the door itself counts as a strange answer,
    /// so a door opened with fewer than 2 strange answers cannot happen; it falls through to TheFuneral.
    /// </summary>
    public static class EndingResolver
    {
        public static Ending Resolve(GameState state)
        {
            if (state == null) return Ending.GoodMorningSam;

            if (state.strangeCount >= GameState.ForkCount) return Ending.SystemError;

            if (state.DoorOpened)
                return state.strangeCount == 3 ? Ending.WakeUp : Ending.TheFuneral;

            return state.strangeCount >= 2 ? Ending.TheNote : Ending.GoodMorningSam;
        }

        /// <summary>Human-readable name for overlays and the END OF DEMO fallback screen.</summary>
        public static string DisplayName(Ending ending)
        {
            switch (ending)
            {
                case Ending.GoodMorningSam: return "Good morning, Sam";
                case Ending.TheNote: return "The note";
                case Ending.TheFuneral: return "The funeral";
                case Ending.WakeUp: return "Wake up";
                case Ending.SystemError: return "System error";
                default: return ending.ToString();
            }
        }
    }
}
