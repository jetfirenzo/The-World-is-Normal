using TMPro;
using TWIN.Util;
using UnityEngine;

namespace TWIN.Endings
{
    /// <summary>
    /// Ending 1, GOOD MORNING, SAM (bad ending, the loop).
    /// Timeline beats: LieDown -> FadeToBlack ("eyes close") -> SayGoodBoy -> (black, silence) -> AlarmRings
    /// (clock 11:11, haptic click, player wakes in the normal bedroom of scene 1) -> FadeFromBlack -> ShowRestartPrompt.
    /// </summary>
    public class GoodMorningSamController : EndingController
    {
        [Header("The normal bedroom in this scene")]
        public AudioSource alarmAudio;
        public Renderer alarmRenderer;
        public TMP_Text clockText;

        protected override void OnEndingStart()
        {
            if (clockText != null) clockText.text = "07:00";
            if (alarmAudio != null) { alarmAudio.clip = ToneGenerator.Alarm(); alarmAudio.loop = true; }
            if (alarmRenderer != null) alarmRenderer.material.color = Color.gray;
        }

        public void SayGoodBoy() => SayAs("Narrator (warm)", GM.lines.goodBoySam, 2.5f);

        /// <summary>The alarm rings, the clock shows 11:11, the player is back in the normal bedroom.</summary>
        public void AlarmRings()
        {
            TeleportToStandPoint();
            if (clockText != null) clockText.text = "11:11";
            if (alarmRenderer != null) alarmRenderer.material.color = Color.red;
            if (alarmAudio != null) alarmAudio.Play();
            if (Haptics != null) Haptics.Click();
        }

        public void AlarmStops()
        {
            if (alarmAudio != null) alarmAudio.Stop();
            if (alarmRenderer != null) alarmRenderer.material.color = Color.gray;
        }
    }
}
