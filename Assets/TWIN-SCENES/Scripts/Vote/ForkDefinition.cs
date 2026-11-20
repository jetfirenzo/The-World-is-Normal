using UnityEngine;

namespace TWIN.Vote
{
    /// <summary>
    /// One fork the audience votes on. Four assets live in Assets/TWIN/Data/Forks.
    /// Edit the labels there; the index must stay 0..3 and unique.
    /// </summary>
    [CreateAssetMenu(menuName = "TWIN/Fork Definition", fileName = "Fork")]
    public class ForkDefinition : ScriptableObject
    {
        [Tooltip("0 = fork 1 (bathroom) ... 3 = fork 4 (last morning)")]
        public int forkIndex;
        public string title = "What does Sam do?";
        [Tooltip("Shown on button 1 / key 1")]
        public string normalLabel = "Normal";
        [Tooltip("Shown on button 2 / key 2")]
        public string strangeLabel = "Strange";
        [Tooltip("Seconds the audience has to vote")]
        public float voteSeconds = 10f;
        [Tooltip("If nobody votes before the timer ends, STRANGE wins when true, NORMAL otherwise")]
        public bool strangeWinsOnTimeout;
    }
}
