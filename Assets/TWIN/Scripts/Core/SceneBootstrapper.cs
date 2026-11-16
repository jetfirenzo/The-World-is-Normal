using UnityEngine;

namespace TWIN.Core
{
    /// <summary>
    /// Put one of these in every scene. If the scene is played directly (teammate presses Play in
    /// 04_Street for example) it spawns the persistent prefab so GameManager, narrator and the fork UI exist.
    /// Optional: a debug state to start from, so you can test a branch without playing from scene 1.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneBootstrapper : MonoBehaviour
    {
        public const string PersistentPrefabResource = "TWIN_Persistent";

        [Header("Only used when this scene is played standalone")]
        public bool applyDebugState;
        public int debugStrangeCount;
        public Route debugRoute = Route.A;
        public bool debugNotePocketed;
        public bool debugFork3Strange;

        void Awake()
        {
            if (GameManager.Instance != null) return;

            var prefab = Resources.Load<GameObject>(PersistentPrefabResource);
            if (prefab == null)
            {
                Debug.LogError($"[TWIN] Resources/{PersistentPrefabResource}.prefab is missing. Run Tools > TWIN > Build All Greybox Scenes.");
                return;
            }
            var go = Instantiate(prefab);
            go.name = PersistentPrefabResource;

            var gm = go.GetComponent<GameManager>();
            if (gm != null && applyDebugState)
            {
                gm.State.strangeCount = Mathf.Clamp(debugStrangeCount, 0, GameState.ForkCount);
                gm.State.route = debugRoute;
                gm.State.notePocketed = debugNotePocketed;
                gm.State.forkWasStrange[2] = debugFork3Strange;
                Debug.Log($"[TWIN] Standalone scene with debug state: {gm.State}");
            }
        }
    }
}
