using TWIN.Util;
using UnityEngine;

namespace TWIN.Endings
{
    /// <summary>
    /// Ending 3, THE FUNERAL (dark).
    /// Timeline beats: OpenDoor (organ hum, heartbeat starts) -> WalkToStandPoint (to the coffin) -> MotherTurns
    /// (Animation track turns her; heartbeat ramps up) -> SayMorningSam -> CutToBlack -> ShowRestartPrompt.
    /// </summary>
    public class TheFuneralController : EndingController
    {
        [Header("Props in this scene")]
        public AudioSource organ;
        public Transform mother;

        [Header("Heartbeat")]
        public float heartbeatStartBpm = 60f;
        public float heartbeatEndBpm = 140f;
        public float heartbeatRampSeconds = 5f;

        protected override void OnEndingStart()
        {
            if (organ != null) { organ.clip = ToneGenerator.Organ(); organ.loop = true; organ.volume = 0.35f; }
        }

        public void OrganOn() { if (organ != null) organ.Play(); }

        public void HeartbeatStarts() { if (Haptics != null) Haptics.StartHeartbeat(heartbeatStartBpm, heartbeatStartBpm, 0f); }

        /// <summary>Called when the mother starts turning: the heartbeat speeds up over the turn.</summary>
        public void HeartbeatRamps() { if (Haptics != null) Haptics.StartHeartbeat(heartbeatStartBpm, heartbeatEndBpm, heartbeatRampSeconds); }

        public void SayMorningSam() => SayAs("Mother", GM.lines.morningSam, 2.5f);
    }
}
