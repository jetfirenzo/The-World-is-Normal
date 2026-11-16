using System;
using TWIN.Core;
using TWIN.Player;
using UnityEngine;
using UnityEngine.Events;

namespace TWIN.Vote
{
    /// <summary>
    /// Runs a fork: asks the vote provider, writes the result into GameState, tells the scene.
    /// Scenes call GameManager.Instance.forkController.RunFork(index, callback).
    /// </summary>
    public class ForkController : MonoBehaviour
    {
        [Tooltip("A MonoBehaviour that implements IVoteProvider. Swap this to plug in the live audience.")]
        public MonoBehaviour providerBehaviour;

        [Serializable] public class ForkResultEvent : UnityEvent<int, bool> { }
        [Tooltip("Fires (forkIndex, strange) after every fork, for logging / overlays / audio.")]
        public ForkResultEvent onForkResolved = new ForkResultEvent();

        public bool IsVoting { get; private set; }
        public int CurrentForkIndex { get; private set; } = -1;

        IVoteProvider Provider => providerBehaviour as IVoteProvider;

        /// <summary>forkIndex is 0-based. onDone(strange) runs after the state has been updated.</summary>
        public void RunFork(int forkIndex, Action<bool> onDone)
        {
            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[TWIN] No GameManager"); return; }
            if (IsVoting) { Debug.LogWarning("[TWIN] A fork is already running"); return; }
            if (Provider == null) { Debug.LogError("[TWIN] ForkController has no IVoteProvider"); return; }

            var def = forkIndex >= 0 && forkIndex < gm.forks.Length ? gm.forks[forkIndex] : null;
            if (def == null) { Debug.LogError($"[TWIN] No ForkDefinition for fork {forkIndex + 1}"); return; }

            if (gm.State.forkAnswered[forkIndex])
            {
                // Scene replayed via the debug keys: reuse the recorded answer instead of voting twice.
                Debug.Log($"[TWIN] Fork {forkIndex + 1} already answered, reusing result");
                onDone?.Invoke(gm.State.forkWasStrange[forkIndex]);
                return;
            }

            IsVoting = true;
            CurrentForkIndex = forkIndex;
            DesktopRig.SetCursorFree(true);
            Provider.StartVote(def, strange =>
            {
                IsVoting = false;
                CurrentForkIndex = -1;
                DesktopRig.SetCursorFree(false);
                gm.State.ApplyFork(forkIndex, strange);
                Debug.Log($"[TWIN] Fork {forkIndex + 1}: {(strange ? "STRANGE" : "NORMAL")} -> {gm.State}");
                onForkResolved.Invoke(forkIndex, strange);
                onDone?.Invoke(strange);
            });
        }

        public void Cancel()
        {
            if (!IsVoting) return;
            Provider?.CancelVote();
            IsVoting = false;
            CurrentForkIndex = -1;
            DesktopRig.SetCursorFree(false);
        }
    }
}
