using System.Collections;
using UnityEngine;

namespace TWIN.Endings
{
    /// <summary>
    /// Ending 4, WAKE UP (good ending).
    /// Timeline beats: OpenDoor (onto pure white) + StopHaptics -> WalkToStandPoint -> FadeToWhiteAndSilence
    /// -> ShowTakeOffHeadset + EnablePassthrough -> ShowRestartPrompt.
    /// </summary>
    public class WakeUpController : EndingController
    {
        [Header("Durations (seconds)")]
        public float silenceSeconds = 3f;
        public float whiteSeconds = 3f;

        public void FadeToWhiteAndSilence()
        {
            if (Fader != null) StartCoroutine(Fader.FadeOut(whiteSeconds, Color.white));
            StartCoroutine(FadeSound());
        }

        IEnumerator FadeSound()
        {
            float start = AudioListener.volume;
            for (float t = 0f; t < silenceSeconds; t += Time.unscaledDeltaTime)
            {
                AudioListener.volume = Mathf.Lerp(start, 0f, t / silenceSeconds);
                yield return null;
            }
            AudioListener.volume = 0f;
        }

        public void ShowTakeOffHeadset() { if (GM != null) GM.ShowCenterText(GM.lines.takeOffHeadset); }

        /// <summary>
        /// Switch the Quest 3 to passthrough so the player sees the real room and the audience.
        /// NOT IMPLEMENTED YET: passthrough needs the Meta OpenXR feature "Passthrough" enabled in
        /// Project Settings > XR Plug-in Management > OpenXR > Meta Quest, a camera with a transparent
        /// background (alpha 0, no skybox) and the AR Foundation ARCameraManager / Composition Layers
        /// passthrough layer (com.unity.xr.meta-openxr is in the project). It cannot be tested on this desktop
        /// greybox, so this hook only logs. The Timeline already calls it at the right moment.
        /// </summary>
        public void EnablePassthrough()
        {
            Debug.Log("[TWIN] WakeUp: EnablePassthrough() called (not implemented yet, see comment)");
        }
    }
}
