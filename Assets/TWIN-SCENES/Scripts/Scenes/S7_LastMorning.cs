using System.Collections;
using TMPro;
using TWIN.Core;
using TWIN.Player;
using TWIN.Util;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 7: the bedroom, wrong (furniture halfway up the walls). The alarm rings.
    /// If notePocketed: the note is in the player's hand and reads "Don't go back to sleep."
    /// If strangeCount >= 2 OR notePocketed: a door has appeared in the wall.
    /// FORK 4 only if the door exists: go back to bed (normal) / open the door (strange). No door = no vote.
    /// </summary>
    public class S7_LastMorning : MonoBehaviour
    {
        public Interactable alarm;
        public Renderer alarmRenderer;
        public AudioSource alarmAudio;
        public TMP_Text clockText;
        public GameObject handNote;
        public TMP_Text handNoteText;
        public GameObject appearedDoor;
        public Interactable appearedDoorInteractable;
        public Interactable bed;

        void Start()
        {
            var gm = GameManager.Instance;
            var s = gm.State;
            if (clockText != null) clockText.text = "11:11";

            bool hasNote = s.notePocketed;
            if (handNote != null) handNote.SetActive(hasNote);
            if (hasNote && handNoteText != null) handNoteText.text = gm.lines.noteTextLastMorning;
            if (hasNote && handNote != null && Camera.main != null)
            {
                handNote.transform.SetParent(Camera.main.transform, false);
                handNote.transform.localPosition = new Vector3(0.25f, -0.25f, 0.5f);
                handNote.transform.localRotation = Quaternion.Euler(20f, -15f, 0f);
            }

            bool hasDoor = s.DoorAppearsInLastMorning;
            if (appearedDoor != null) appearedDoor.SetActive(hasDoor);
            if (appearedDoorInteractable != null) { appearedDoorInteractable.available = false; appearedDoorInteractable.prompt = "Open the door"; appearedDoorInteractable.onInteract.AddListener(() => StartCoroutine(Finish("opened the door"))); }
            if (bed != null) { bed.available = false; bed.prompt = "Go back to bed"; bed.onInteract.AddListener(() => StartCoroutine(Finish("went back to bed"))); }

            if (alarmAudio != null) { alarmAudio.clip = ToneGenerator.Alarm(); alarmAudio.loop = true; alarmAudio.Play(); }
            if (alarmRenderer != null) alarmRenderer.material.color = Color.red;
            if (alarm != null) { alarm.prompt = "Turn off the alarm"; alarm.onInteract.AddListener(() => StartCoroutine(AfterAlarm(hasDoor))); }
            Debug.Log($"[TWIN] Last morning: note={hasNote} door={hasDoor} ({s})");
        }

        IEnumerator AfterAlarm(bool hasDoor)
        {
            var gm = GameManager.Instance;
            if (alarmAudio != null) alarmAudio.Stop();
            if (alarmRenderer != null) alarmRenderer.material.color = Color.gray;
            yield return new WaitForSeconds(1.5f);

            if (!hasDoor)
            {
                // No door = no vote. Sam goes back to bed.
                gm.narrator.Say(gm.lines.backToSleep);
                yield return gm.narrator.WaitUntilDone();
                if (bed != null) bed.available = true;
                yield break;
            }

            bool done = false, strange = false;
            gm.forkController.RunFork(3, r => { strange = r; done = true; });
            while (!done) yield return null;

            if (strange) { if (appearedDoorInteractable != null) appearedDoorInteractable.available = true; }
            else
            {
                gm.narrator.Say(gm.lines.backToSleep);
                if (bed != null) bed.available = true;
            }
        }

        IEnumerator Finish(string what)
        {
            Debug.Log($"[TWIN] Sam {what}. Demo ends.");
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.GoToNextScene();
        }
    }
}
