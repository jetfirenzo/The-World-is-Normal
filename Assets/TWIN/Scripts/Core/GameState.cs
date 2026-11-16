using System;

namespace TWIN.Core
{
    /// <summary>Which branch the story took at fork 2 (living room).</summary>
    public enum Route
    {
        None,
        A, // looked for milk -> supermarket
        B  // answered the phone -> florist
    }

    /// <summary>
    /// The complete game state. This is the ONLY place story variables live.
    /// Scenes read it through GameManager.Instance.State and change it only via ApplyFork / SetRoute.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public const int ForkCount = 4;

        /// <summary>How many forks were answered with the STRANGE option (0-4).</summary>
        public int strangeCount;

        /// <summary>Route A or B, decided at fork 2. None before that.</summary>
        public Route route = Route.None;

        /// <summary>True if the note from the bathroom was pocketed at fork 1.</summary>
        public bool notePocketed;

        /// <summary>Per-fork record: true = strange was chosen. Index 0..3 = fork 1..4. </summary>
        public bool[] forkWasStrange = new bool[ForkCount];

        /// <summary>Per-fork record: true once the fork has been answered.</summary>
        public bool[] forkAnswered = new bool[ForkCount];

        /// <summary>
        /// Apply the audience result of a fork. forkIndex is 0-based (0 = fork 1).
        /// Side effects per fork are applied here so no scene script can forget them:
        ///  fork 0 strange -> notePocketed
        ///  fork 1 normal -> route A, strange -> route B
        /// </summary>
        public void ApplyFork(int forkIndex, bool strange)
        {
            if (forkIndex < 0 || forkIndex >= ForkCount)
                throw new ArgumentOutOfRangeException(nameof(forkIndex));
            if (forkAnswered[forkIndex])
                throw new InvalidOperationException($"Fork {forkIndex + 1} was already answered.");

            forkAnswered[forkIndex] = true;
            forkWasStrange[forkIndex] = strange;
            if (strange) strangeCount = Math.Min(ForkCount, strangeCount + 1);

            switch (forkIndex)
            {
                case 0:
                    notePocketed = strange;
                    break;
                case 1:
                    route = strange ? Route.B : Route.A;
                    break;
            }
        }

        /// <summary>Scene 7 rule: a door appears if strangeCount >= 2 OR the note was pocketed.</summary>
        public bool DoorAppearsInLastMorning => strangeCount >= 2 || notePocketed;

        public void Reset()
        {
            strangeCount = 0;
            route = Route.None;
            notePocketed = false;
            forkWasStrange = new bool[ForkCount];
            forkAnswered = new bool[ForkCount];
        }

        public override string ToString()
        {
            return $"strangeCount={strangeCount} route={route} notePocketed={notePocketed}";
        }
    }
}
