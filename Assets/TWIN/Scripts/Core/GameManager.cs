using System.Collections;
using TMPro;
using TWIN.Haptics;
using TWIN.Narration;
using TWIN.Vote;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TWIN.Core
{
    /// <summary>
    /// Persistent singleton (lives on the TWIN_Persistent prefab in Resources, survives scene loads).
    /// Owns the GameState, the scene flow, the narrator, the fork controller, the fader and the haptics.
    /// After fork 4 it loads the resolved ending scene additively (or the END OF DEMO fallback).
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
        public HapticsService haptics;

        [Header("Ending UI")]
        public TMP_Text centerText;
        public TMP_Text restartPromptText;

        [Header("Debug")]
        [Tooltip("ON: after fork 4 show the END OF DEMO screen instead of the resolved ending scene.")]
        public bool useEndOfDemoFallback;

        [Header("Runtime")]
        public GameState State = new GameState();

        bool _loading;

        public string CurrentScene => SceneManager.GetActiveScene().name;
        public bool EndingRunning { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            HideEndingTexts();
        }

        /// <summary>Fade to black and load the next scene in the flow. After the last morning: the ending.</summary>
        public void GoToNextScene()
        {
            if (CurrentScene == flow.lastMorning && !useEndOfDemoFallback)
            {
                LoadEnding(EndingResolver.Resolve(State));
                return;
            }
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

        /// <summary>
        /// Load an ending scene additively on top of the current scene (the last morning stays, so the
        /// ending can reuse its bedroom, bed and door through LastMorningBridge). No fade: the ending's
        /// own Timeline does its fades.
        /// </summary>
        public void LoadEnding(Ending ending)
        {
            if (_loading) return;
            StartCoroutine(LoadEndingRoutine(ending));
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            _loading = true;
            EndingRunning = false;
            if (forkController != null) forkController.Cancel();
            if (narrator != null) narrator.Clear();
            if (haptics != null) haptics.StopLoop();
            if (fader != null) yield return fader.FadeOut(0.6f);
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
            HideEndingTexts();
            if (fader != null) yield return fader.FadeIn(0.6f);
            _loading = false;
        }

        IEnumerator LoadEndingRoutine(Ending ending)
        {
            _loading = true;
            EndingRunning = true;
            string sceneName = flow.EndingScene(ending);
            Debug.Log($"[TWIN] Ending: {ending} -> {sceneName} ({State})");
            if (forkController != null) forkController.Cancel();
            if (narrator != null) narrator.Clear();
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)); // the ending's lighting applies
            _loading = false;
        }

        /// <summary>"Press R to start a new run". R is handled by DebugOverlay -> RestartDemo().</summary>
        public void ShowRestartPrompt()
        {
            if (restartPromptText != null)
            {
                restartPromptText.text = lines != null ? lines.restartPrompt : "Press R to start a new run";
                restartPromptText.gameObject.SetActive(true);
            }
        }

        public void ShowCenterText(string text)
        {
            if (centerText == null) return;
            centerText.text = text;
            centerText.gameObject.SetActive(true);
        }

        public void HideEndingTexts()
        {
            if (centerText != null) centerText.gameObject.SetActive(false);
            if (restartPromptText != null) restartPromptText.gameObject.SetActive(false);
        }

        /// <summary>Start a new run: clean state, scene 1. Also undoes whatever an ending did to audio, fader and haptics.</summary>
        public void RestartDemo()
        {
            State.Reset();
            AudioListener.volume = 1f;
            if (haptics != null) haptics.StopAll();
            if (fader != null) fader.ResetToBlack();
            HideEndingTexts();
            LoadScene(flow.bedroom);
        }
    }
}
