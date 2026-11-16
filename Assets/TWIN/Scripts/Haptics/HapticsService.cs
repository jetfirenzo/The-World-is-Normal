using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace TWIN.Haptics
{
    /// <summary>
    /// One place for controller haptics. On a headset it sends impulses to the left/right XR controllers;
    /// on desktop it only logs (and shows in the debug overlay), so endings can be tested without hardware.
    /// "Chest" haptics do not exist on Quest controllers: Heartbeat() plays on both hands.
    /// Lives on the TWIN_Persistent prefab; use HapticsService.Instance.
    /// </summary>
    public class HapticsService : MonoBehaviour
    {
        public static HapticsService Instance { get; private set; }

        public enum Hand { Left, Right, Both }

        [Tooltip("Scales every impulse (0 = silent)")]
        [Range(0f, 1f)] public float masterStrength = 1f;

        public string LastEvent { get; private set; } = "";

        Coroutine _loop;
        readonly List<InputDevice> _devices = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>One impulse. amplitude 0..1, seconds > 0.</summary>
        public void Pulse(float amplitude, float seconds, Hand hand = Hand.Both)
        {
            amplitude = Mathf.Clamp01(amplitude * masterStrength);
            Log($"pulse {amplitude:0.00} x {seconds:0.00}s {hand}");
            Send(hand, amplitude, seconds);
        }

        /// <summary>A short sharp click (the alarm clock button).</summary>
        public void Click(Hand hand = Hand.Both) => Pulse(0.8f, 0.05f, hand);

        /// <summary>Repeat a soft pulse until StopLoop(). Used while the hand writes the note.</summary>
        public void StartSlowPulse(float amplitude = 0.3f, float pulseSeconds = 0.25f, float intervalSeconds = 0.8f, Hand hand = Hand.Both)
        {
            StopLoop();
            Log($"slow pulse start ({intervalSeconds:0.00}s)");
            _loop = StartCoroutine(PulseLoop(amplitude, pulseSeconds, () => intervalSeconds, hand));
        }

        /// <summary>
        /// Heartbeat (double thump) that ramps from startBpm to endBpm over rampSeconds, then holds, until StopLoop().
        /// </summary>
        public void StartHeartbeat(float startBpm = 60f, float endBpm = 140f, float rampSeconds = 6f, float amplitude = 0.6f)
        {
            StopLoop();
            Log($"heartbeat {startBpm}->{endBpm} bpm over {rampSeconds}s");
            float t0 = Time.time;
            _loop = StartCoroutine(HeartbeatLoop(amplitude, () =>
            {
                float f = rampSeconds <= 0f ? 1f : Mathf.Clamp01((Time.time - t0) / rampSeconds);
                return Mathf.Lerp(startBpm, endBpm, f);
            }));
        }

        public void StopLoop()
        {
            if (_loop != null) StopCoroutine(_loop);
            _loop = null;
        }

        /// <summary>Stop every loop and send a zero impulse to both controllers.</summary>
        public void StopAll()
        {
            StopLoop();
            Log("stop all");
            Send(Hand.Both, 0f, 0.01f);
        }

        IEnumerator PulseLoop(float amplitude, float pulseSeconds, System.Func<float> interval, Hand hand)
        {
            while (true)
            {
                Send(hand, amplitude * masterStrength, pulseSeconds);
                yield return new WaitForSeconds(interval());
            }
        }

        IEnumerator HeartbeatLoop(float amplitude, System.Func<float> bpm)
        {
            while (true)
            {
                float period = 60f / Mathf.Max(20f, bpm());
                Send(Hand.Both, amplitude * masterStrength, 0.08f);          // lub
                yield return new WaitForSeconds(period * 0.3f);
                Send(Hand.Both, amplitude * 0.6f * masterStrength, 0.06f);   // dub
                yield return new WaitForSeconds(period * 0.7f);
            }
        }

        void Send(Hand hand, float amplitude, float seconds)
        {
            if (!XRSettings.isDeviceActive) return; // desktop: logging only
            if (hand == Hand.Left || hand == Hand.Both) SendTo(InputDeviceCharacteristics.Left, amplitude, seconds);
            if (hand == Hand.Right || hand == Hand.Both) SendTo(InputDeviceCharacteristics.Right, amplitude, seconds);
        }

        void SendTo(InputDeviceCharacteristics side, float amplitude, float seconds)
        {
            _devices.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | side, _devices);
            foreach (var d in _devices)
            {
                if (d.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
                    d.SendHapticImpulse(0, amplitude, seconds);
            }
        }

        void Log(string what)
        {
            LastEvent = what;
            Debug.Log($"[Haptics] {what}");
        }
    }
}
