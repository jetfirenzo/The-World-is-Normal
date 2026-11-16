namespace TWIN.Core
{
    /// <summary>
    /// Decides which ending WOULD play. The ending scenes themselves are not built yet;
    /// the END OF DEMO screen only shows the name this returns.
    ///
    /// TODO(story): the brief was cut off after the first rule. Only that rule is implemented.
    /// Add the remaining rules below, in priority order, and extend EndingResolverTests.
    /// See docs/ENDINGS-TODO.md.
    /// </summary>
    public static class EndingResolver
    {
        public const string SystemError = "System error";
        public const string Unknown = "TODO: ending rules not yet specified";

        public static string Resolve(GameState state)
        {
            if (state == null) return Unknown;

            // Rule 1 (from the brief): strangeCount == 4 -> "System error" (overrides everything else).
            if (state.strangeCount == GameState.ForkCount) return SystemError;

            // TODO(story): remaining rules go here, e.g.
            // if (state.route == Route.A && ...) return "...";
            return Unknown;
        }
    }
}
