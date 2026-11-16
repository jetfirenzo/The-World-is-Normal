using System.Collections;
using TWIN.Core;
using TWIN.Haptics;
using TWIN.Narration;
using TWIN.Player;
using TWIN.Scenes;
using UnityEngine;
using UnityEngine.Playables;

namespace TWIN.Endings
{
    /// <summary>
    /// Base of the five ending scripts. Each ending scene has one controller, a PlayableDirector with a
    /// Timeline whose Signal track calls the public methods below (and the subclass's), so the timing
    /// lives in the Timeline asset (Assets/TWIN/Data/Endings/*.playable) and can be changed without code.
    /// Durations that are not a Timeline clip (a fade, a walk) are fields here, tweakable in the Inspector.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public abstract class EndingController : MonoBehaviour
    {
        public Ending ending;
        public PlayableDirector director;

        [Header("Scene references")]
        [Tooltip("Where the player is put / walked to in this ending's own room")]
        public Transform standPoint;
        [Tooltip("The last morning's north wall replaced by this wall with a doorway (endings that open the door)")]
        public GameObject doorwayWall;

        [Header("Durations (seconds)")]
        public float fadeSeconds = 3f;
        public float walkSeconds = 4f;
        public float doorSwingSeconds = 1.5f;

        protected GameManager GM => GameManager.Instance;
        protected Narrator Narrator => GM != null ? GM.narrator : null;
        protected ScreenFader Fader => GM != null ? GM.fader : null;
        protected HapticsService Haptics => HapticsService.Instance;
        protected LastMorningBridge Bridge { get; private set; }
        protected DesktopRig Rig { get; private set; }

        protected virtual void Awake()
        {
            if (director == null) director = GetComponent<PlayableDirector>();
            if (doorwayWall != null) doorwayWall.SetActive(false);
        }

        protected virtual void Start()
        {
            Bridge = FindFirstObjectByType<LastMorningBridge>();
            Rig = DesktopRig.Current;
            if (Bridge == null) Debug.LogWarning($"[TWIN] {name}: no LastMorningBridge (ending played standalone?). Bed/door beats are skipped.");
            if (Rig != null) Rig.enabled = false; // the ending moves the player, not the keyboard
            if (Bridge != null && Bridge.handNote != null) Bridge.handNote.SetActive(false);
            DesktopRig.SetCursorFree(false);
            if (Narrator != null) Narrator.Clear();
            OnEndingStart();
            if (director != null && !director.playOnAwake) director.Play();
            Debug.Log($"[TWIN] Ending started: {ending}");
        }

        /// <summary>Subclass setup before the Timeline starts.</summary>
        protected virtual void OnEndingStart() { }

        // ---------------------------------------------------------------- Timeline signal targets (parameterless)

        /// <summary>Sam lies down in the last morning's bed, looking at the ceiling.</summary>
        public void LieDown()
        {
            if (Bridge != null && Bridge.bedLiePoint != null) MoveRig(Bridge.bedLiePoint.position, Bridge.bedLiePoint.rotation, lookPitch: -80f);
        }

        public void FadeToBlack() { if (Fader != null) StartCoroutine(Fader.FadeOut(fadeSeconds)); }
        public void FadeFromBlack() { if (Fader != null) StartCoroutine(Fader.FadeIn(fadeSeconds)); }
        public void CutToBlack() { if (Fader != null) { Fader.SetColor(Color.black); Fader.SetAlpha(1f); } }

        /// <summary>Put the player on this ending's stand point (instant). The stand point's X rotation is the look pitch.</summary>
        public void TeleportToStandPoint()
        {
            if (standPoint != null) MoveRig(standPoint.position, standPoint.rotation, PitchOf(standPoint));
        }

        static float PitchOf(Transform t)
        {
            float x = t.rotation.eulerAngles.x;
            return x > 180f ? x - 360f : x;
        }

        /// <summary>Open the last morning's door and swap its wall for this ending's wall with a doorway.</summary>
        public void OpenDoor()
        {
            if (doorwayWall != null) doorwayWall.SetActive(true);
            if (Bridge != null) StartCoroutine(Bridge.OpenDoor(doorSwingSeconds));
        }

        /// <summary>Walk the player (scripted) to the stand point over walkSeconds.</summary>
        public void WalkToStandPoint()
        {
            if (standPoint != null) StartCoroutine(WalkRoutine(standPoint.position, standPoint.rotation, walkSeconds));
        }

        public void ShowRestartPrompt() { if (GM != null) GM.ShowRestartPrompt(); }
        public void StopHaptics() { if (Haptics != null) Haptics.StopAll(); }

        // ---------------------------------------------------------------- helpers

        protected void Say(string line, float seconds = -1f) { if (Narrator != null) Narrator.Say(line, seconds); }
        protected void SayAs(string who, string line, float seconds = -1f) { if (Narrator != null) Narrator.SayAs(who, line, seconds); }

        protected void MoveRig(Vector3 position, Quaternion rotation, float lookPitch)
        {
            if (Rig == null) return;
            var cc = Rig.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            Rig.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));
            if (Rig.cameraPivot != null) Rig.cameraPivot.localRotation = Quaternion.Euler(lookPitch, 0f, 0f);
            if (cc != null) cc.enabled = true;
        }

        IEnumerator WalkRoutine(Vector3 to, Quaternion rot, float seconds)
        {
            if (Rig == null) yield break;
            var cc = Rig.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            Vector3 from = Rig.transform.position;
            Quaternion fromRot = Rig.transform.rotation;
            Quaternion toRot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
            Quaternion fromPitch = Rig.cameraPivot != null ? Rig.cameraPivot.localRotation : Quaternion.identity;
            Quaternion toPitch = Quaternion.Euler(standPoint != null ? PitchOf(standPoint) : 0f, 0f, 0f);
            for (float f = 0f; f < 1f; f += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                float e = Mathf.SmoothStep(0f, 1f, f);
                Rig.transform.SetPositionAndRotation(Vector3.Lerp(from, to, e), Quaternion.Slerp(fromRot, toRot, e));
                if (Rig.cameraPivot != null) Rig.cameraPivot.localRotation = Quaternion.Slerp(fromPitch, toPitch, e);
                yield return null;
            }
            Rig.transform.SetPositionAndRotation(to, toRot);
            if (cc != null) cc.enabled = true;
        }
    }
}
