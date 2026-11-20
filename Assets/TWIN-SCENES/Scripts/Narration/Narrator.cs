using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TWIN.Narration
{
    /// <summary>
    /// Shows narrator / character lines as subtitles. Audio is a placeholder: assign an AudioSource
    /// and a clip per line later; for now a short beep marks the start of a line.
    /// </summary>
    public class Narrator : MonoBehaviour
    {
        public TMP_Text subtitle;
        public AudioSource voice;
        [Tooltip("Seconds per character, used when no explicit duration is given")]
        public float secondsPerCharacter = 0.06f;
        public float minimumSeconds = 2f;

        readonly Queue<(string speaker, string line, float seconds)> _queue = new();
        Coroutine _runner;

        public bool IsSpeaking => _runner != null;

        /// <summary>Queue a narrator line. Duration is estimated from the length when seconds <= 0.</summary>
        public void Say(string line, float seconds = -1f) => Enqueue("", line, seconds);

        /// <summary>Queue a line from another speaker ("Phone", "Shopkeeper").</summary>
        public void SayAs(string speaker, string line, float seconds = -1f) => Enqueue(speaker, line, seconds);

        /// <summary>Yield on this to wait until the queue is empty.</summary>
        public IEnumerator WaitUntilDone()
        {
            while (IsSpeaking || _queue.Count > 0) yield return null;
        }

        public void Clear()
        {
            _queue.Clear();
            if (_runner != null) StopCoroutine(_runner);
            _runner = null;
            if (subtitle != null) subtitle.text = "";
        }

        void Enqueue(string speaker, string line, float seconds)
        {
            if (seconds <= 0f) seconds = Mathf.Max(minimumSeconds, line.Length * secondsPerCharacter);
            _queue.Enqueue((speaker, line, seconds));
            if (_runner == null) _runner = StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            while (_queue.Count > 0)
            {
                var (speaker, line, seconds) = _queue.Dequeue();
                if (subtitle != null)
                    subtitle.text = string.IsNullOrEmpty(speaker) ? line : $"<b>{speaker}:</b> {line}";
                if (voice != null) voice.Play();
                Debug.Log($"[Narrator] {(string.IsNullOrEmpty(speaker) ? "" : speaker + ": ")}{line}");
                yield return new WaitForSeconds(seconds);
                if (subtitle != null) subtitle.text = "";
                yield return new WaitForSeconds(0.3f);
            }
            _runner = null;
        }
    }
}
