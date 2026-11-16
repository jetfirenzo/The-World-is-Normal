using System.Collections;
using TMPro;
using UnityEngine;

namespace TWIN.Endings
{
    /// <summary>
    /// Ending 2, THE NOTE (twist, the loop explained).
    /// Timeline beats: LieDown -> FadeToBlack (short) -> ArriveStanding (next to the bed; someone else is in it)
    /// -> HandStartsMoving (Animation track moves the hand to the nightstand; slow haptic pulse) -> WriteNote
    /// (letters appear one by one on the note) -> FigureSitsUp (Animation track) + StopHaptics -> CutToBlack -> ShowRestartPrompt.
    /// </summary>
    public class TheNoteController : EndingController
    {
        [Header("Props in this scene")]
        public TMP_Text noteText;
        public Transform hand;
        public Transform figureInBed;

        [Header("Durations (seconds)")]
        public float writeSeconds = 4f;

        protected override void OnEndingStart()
        {
            if (noteText != null) { noteText.text = GM.lines.noteBeingWritten; noteText.maxVisibleCharacters = 0; }
        }

        public void ArriveStanding()
        {
            TeleportToStandPoint();
            FadeFromBlack();
        }

        public void HandStartsMoving()
        {
            if (Haptics != null) Haptics.StartSlowPulse();
        }

        public void WriteNote() => StartCoroutine(WriteRoutine());

        IEnumerator WriteRoutine()
        {
            if (noteText == null) yield break;
            noteText.ForceMeshUpdate();
            int total = noteText.textInfo.characterCount;
            for (int i = 0; i <= total; i++)
            {
                noteText.maxVisibleCharacters = i;
                yield return new WaitForSeconds(writeSeconds / Mathf.Max(1, total));
            }
        }
    }
}
