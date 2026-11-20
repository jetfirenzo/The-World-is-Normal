using System.Collections;
using TWIN.Core;
using TWIN.Player;
using TWIN.Util;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 3: table set for one, bowl of cornflakes. Narrator: breakfast line.
    /// FORK 2: look for milk (normal, route A) / answer the ringing phone (strange, route B).
    /// Route A: the fridge holds only more bowls of cornflakes. Route B: the phone call.
    /// </summary>
    public class S3_LivingRoom : MonoBehaviour
    {
        public Interactable fridge;
        public Transform fridgeDoor;
        public GameObject fridgeContents;
        public Interactable phone;
        public AudioSource phoneAudio;
        public Door frontDoor;

        void Start()
        {
            if (fridge != null) { fridge.available = false; fridge.prompt = "Look for milk"; fridge.onInteract.AddListener(() => StartCoroutine(RouteA())); }
            if (phone != null) { phone.available = false; phone.prompt = "Answer the phone"; phone.onInteract.AddListener(() => StartCoroutine(RouteB())); }
            if (fridgeContents != null) fridgeContents.SetActive(false);
            if (frontDoor != null) frontDoor.Lock();
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            var gm = GameManager.Instance;
            yield return new WaitForSeconds(1f);
            gm.narrator.Say(gm.lines.breakfast);
            yield return gm.narrator.WaitUntilDone();

            // The phone starts ringing just before the vote so both options are visible.
            if (phoneAudio != null) { phoneAudio.clip = ToneGenerator.RingTone(); phoneAudio.loop = true; phoneAudio.Play(); }

            bool done = false, strange = false;
            gm.forkController.RunFork(1, s => { strange = s; done = true; });
            while (!done) yield return null;

            if (strange)
            {
                if (phone != null) phone.available = true;
            }
            else
            {
                if (phoneAudio != null) phoneAudio.Stop();
                if (fridge != null) fridge.available = true;
            }
        }

        IEnumerator RouteA()
        {
            var gm = GameManager.Instance;
            if (fridgeDoor != null) yield return Swing(fridgeDoor, 100f, 0.6f);
            if (fridgeContents != null) fridgeContents.SetActive(true);
            yield return new WaitForSeconds(1f);
            gm.narrator.Say(gm.lines.outOfMilk);
            yield return gm.narrator.WaitUntilDone();
            if (frontDoor != null) frontDoor.Unlock();
        }

        IEnumerator RouteB()
        {
            var gm = GameManager.Instance;
            if (phoneAudio != null) phoneAudio.Stop();
            yield return new WaitForSeconds(0.8f);
            gm.narrator.SayAs("Voice on the phone", gm.lines.phoneVoice, 3.5f);
            gm.narrator.SayAs("Narrator (cheerfully)", gm.lines.howSad, 3.5f);
            yield return gm.narrator.WaitUntilDone();
            if (frontDoor != null) frontDoor.Unlock();
        }

        public static IEnumerator Swing(Transform hinge, float degrees, float seconds)
        {
            var from = hinge.localRotation;
            var to = from * Quaternion.Euler(0f, degrees, 0f);
            for (float f = 0f; f < 1f; f += Time.deltaTime / seconds)
            {
                hinge.localRotation = Quaternion.Slerp(from, to, f);
                yield return null;
            }
            hinge.localRotation = to;
        }
    }
}
