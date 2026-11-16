using System.Collections;
using TMPro;
using TWIN.Core;
using TWIN.Player;
using TWIN.Util;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 1: the alarm rings, the player clicks it off. Silence. The clock flips to 11:11 and rings once
    /// more by itself. Narrator: "Good morning, Sam." Then the door unlocks.
    /// </summary>
    public class S1_Bedroom : MonoBehaviour
    {
        public Interactable alarm;
        public Renderer alarmRenderer;
        public AudioSource alarmAudio;
        public TMP_Text clockText;
        public Door door;

        [Header("Timing")]
        public float silenceSeconds = 2.5f;
        public float secondRingSeconds = 1.5f;

        void Start()
        {
            if (clockText != null) clockText.text = "07:00";
            if (alarmAudio != null)
            {
                alarmAudio.clip = ToneGenerator.Alarm();
                alarmAudio.loop = true;
                alarmAudio.Play();
            }
            if (alarmRenderer != null) alarmRenderer.material.color = Color.red;
            if (alarm != null)
            {
                alarm.prompt = "Turn off the alarm";
                alarm.onInteract.AddListener(() => StartCoroutine(AfterAlarm()));
            }
            if (door != null) door.Lock();
        }

        IEnumerator AfterAlarm()
        {
            if (alarmAudio != null) alarmAudio.Stop();
            if (alarmRenderer != null) alarmRenderer.material.color = Color.gray;

            yield return new WaitForSeconds(silenceSeconds);

            // The clock flips back to 11:11 by itself.
            if (clockText != null) yield return FlipClock("11:11");

            // ...and rings once more.
            if (alarmRenderer != null) alarmRenderer.material.color = Color.red;
            if (alarmAudio != null) alarmAudio.Play();
            yield return new WaitForSeconds(secondRingSeconds);
            if (alarmAudio != null) alarmAudio.Stop();
            if (alarmRenderer != null) alarmRenderer.material.color = Color.gray;

            var gm = GameManager.Instance;
            gm.narrator.Say(gm.lines.goodMorning);
            yield return gm.narrator.WaitUntilDone();

            if (door != null) door.Unlock();
        }

        IEnumerator FlipClock(string newText)
        {
            var t = clockText.transform;
            var s = t.localScale;
            for (float f = 0f; f < 1f; f += Time.deltaTime * 6f)
            {
                t.localScale = new Vector3(s.x, s.y * (1f - f), s.z);
                yield return null;
            }
            clockText.text = newText;
            for (float f = 0f; f < 1f; f += Time.deltaTime * 6f)
            {
                t.localScale = new Vector3(s.x, s.y * f, s.z);
                yield return null;
            }
            t.localScale = s;
        }
    }
}
