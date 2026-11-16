using UnityEngine;

namespace TWIN.Util
{
    /// <summary>Placeholder audio: generates short clips in memory so the greybox has no audio assets.</summary>
    public static class ToneGenerator
    {
        static AudioClip s_alarm, s_blip, s_ringTone;

        /// <summary>Pulsed 880 Hz alarm, 1 second, loopable.</summary>
        public static AudioClip Alarm()
        {
            if (s_alarm == null) s_alarm = Make("TWIN_Alarm", 1.0f, t => Mathf.Sin(2f * Mathf.PI * 880f * t) * (Mathf.Repeat(t, 0.25f) < 0.12f ? 0.5f : 0f));
            return s_alarm;
        }

        /// <summary>Soft 440 Hz blip, 0.15 s, used when a subtitle starts.</summary>
        public static AudioClip Blip()
        {
            if (s_blip == null) s_blip = Make("TWIN_Blip", 0.15f, t => Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.3f * (1f - t / 0.15f));
            return s_blip;
        }

        /// <summary>Old phone ring (two tones), 1.5 s, loopable.</summary>
        public static AudioClip RingTone()
        {
            if (s_ringTone == null) s_ringTone = Make("TWIN_Ring", 1.5f, t => Mathf.Repeat(t, 1.5f) < 0.8f
                ? (Mathf.Sin(2f * Mathf.PI * 1000f * t) + Mathf.Sin(2f * Mathf.PI * 1200f * t)) * 0.2f * (Mathf.Sin(2f * Mathf.PI * 20f * t) > 0 ? 1f : 0.3f)
                : 0f);
            return s_ringTone;
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            const int rate = 44100;
            int n = Mathf.CeilToInt(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = f(i / (float)rate);
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
