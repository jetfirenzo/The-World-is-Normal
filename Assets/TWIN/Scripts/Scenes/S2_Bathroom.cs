using System.Collections;
using TMPro;
using TWIN.Core;
using TWIN.Player;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 2: a note on the sink reads "Your name isn't Sam." The mirror shows no reflection (MirrorNoReflection).
    /// FORK 1: throw the note in the bin (normal) / put it in your pocket (strange, notePocketed).
    /// </summary>
    public class S2_Bathroom : MonoBehaviour
    {
        public Interactable note;
        public TMP_Text noteText;
        public Transform bin;
        public Door door;

        void Start()
        {
            var gm = GameManager.Instance;
            if (noteText != null) noteText.text = gm.lines.noteText;
            if (note != null)
            {
                note.prompt = "Read the note";
                note.onInteract.AddListener(() => StartCoroutine(ReadNote()));
            }
            if (door != null) door.Lock();
        }

        IEnumerator ReadNote()
        {
            var gm = GameManager.Instance;
            gm.narrator.SayAs("Note", gm.lines.noteText, 3f);
            yield return gm.narrator.WaitUntilDone();

            bool done = false, strange = false;
            gm.forkController.RunFork(0, s => { strange = s; done = true; });
            while (!done) yield return null;

            if (strange) yield return MoveNoteTo(Camera.main != null ? Camera.main.transform : transform, 0.5f);
            else if (bin != null) yield return MoveNoteTo(bin, 0.7f);
            if (note != null) note.gameObject.SetActive(false);

            if (door != null) door.Unlock();
        }

        IEnumerator MoveNoteTo(Transform target, float seconds)
        {
            if (note == null) yield break;
            var t = note.transform;
            var from = t.position;
            for (float f = 0f; f < 1f; f += Time.deltaTime / seconds)
            {
                t.position = Vector3.Lerp(from, target.position + Vector3.down * 0.2f, f);
                yield return null;
            }
        }
    }
}
