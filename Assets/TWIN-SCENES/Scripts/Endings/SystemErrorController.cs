using System.Collections;
using UnityEngine;

namespace TWIN.Endings
{
    /// <summary>
    /// Ending 5, SYSTEM ERROR (all four forks strange).
    /// TODO(story): the brief does not describe this ending yet. Placeholder: the screen glitches
    /// (black flicker), "SYSTEM ERROR" appears, then the restart prompt. Replace the beats in
    /// Assets/TWIN/Data/Endings/End_SystemError.playable and this class once the ending is written.
    /// </summary>
    public class SystemErrorController : EndingController
    {
        [Header("Durations (seconds)")]
        public float glitchSeconds = 2f;

        public void Glitch() => StartCoroutine(GlitchRoutine());

        IEnumerator GlitchRoutine()
        {
            if (Fader == null) yield break;
            Fader.SetColor(Color.black);
            float end = Time.time + glitchSeconds;
            while (Time.time < end)
            {
                Fader.SetAlpha(Random.value > 0.5f ? 1f : 0f);
                if (Haptics != null && Random.value > 0.7f) Haptics.Pulse(Random.Range(0.2f, 1f), 0.05f);
                yield return new WaitForSeconds(Random.Range(0.03f, 0.15f));
            }
            Fader.SetAlpha(1f);
        }

        public void ShowSystemError() { if (GM != null) GM.ShowCenterText($"<color=#ff3030>{GM.lines.systemError}</color>"); }
    }
}
