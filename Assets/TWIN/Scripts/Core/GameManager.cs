using System.Collections;
using TWIN.Narration;
using TWIN.Vote;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TWIN.Core
{
    /// <summary>
    /// Persistent singleton (lives on the TWIN_Persistent prefab in Resources, survives scene loads).
    /// Owns the GameState, the scene flow, the narrator, the fork controller and the fader.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Data")]
        public SceneFlow flow;
        public NarratorLines lines;
        public ForkDefinition[] forks = new ForkDefinition[GameState.ForkCount];

        [Header("Services (children of this prefab)")]
        public Narrator narrator;
        public ForkController forkController;
        public ScreenFader fader;

        [Header("Runtime")]
        public GameState State = new GameState();

        bool _loading;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public string CurrentScene => SceneManager.GetActiveScene().name;

        /// <summary>Fade to black and load the next scene in the flow.</summary>
        public void GoToNextScene()
        {
            var next = flow.NextAfter(CurrentScene, State);
            if (string.IsNullOrEmpty(next))
            {
                Debug.LogWarning($"[TWIN] No scene after {CurrentScene}");
                return;
            }
            LoadScene(next);
        }

        public void LoadScene(string sceneName)
        {
            if (_loading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            _loading = true;
            if (forkController != null) forkController.Cancel();
            if (narrator != null) narrator.Clear();
            if (fader != null) yield return fader.FadeOut(0.6f);
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
            if (fader != null) yield return fader.FadeIn(0.6f);
            _loading = false;
        }

        /// <summary>Restart the whole demo with a clean state.</summary>
        public void RestartDemo()
        {
            State.Reset();
            LoadScene(flow.boot);
        }
    }
}
