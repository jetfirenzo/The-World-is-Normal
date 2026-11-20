using System.Collections;
using TWIN.Core;
using TWIN.Player;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 5: ONE room, two dressings chosen by route (supermarket / florist).
    /// Shopkeeper (same face as the child): "That's free. Everything is free today."
    /// FORK 3: take it and go home (normal -> 06A) / look behind the counter (strange -> 06B).
    /// </summary>
    public class S5_Shop : MonoBehaviour
    {
        public GameObject supermarketDressing;
        public GameObject floristDressing;
        public Interactable shopkeeper;
        public Interactable itemSupermarket;
        public Interactable itemFlorist;
        public Interactable counterBack;
        public GameObject counterPanel;
        public GameObject backroomsOpening;
        public Door frontDoor;
        public Door backroomsDoor;

        Interactable _item;

        void Start()
        {
            var gm = GameManager.Instance;
            var route = gm.State.route;
            if (route == Route.None)
            {
                Debug.LogWarning("[TWIN] Shop entered without a route (standalone play?). Using route A.");
                route = Route.A;
            }
            bool a = route == Route.A;
            if (supermarketDressing != null) supermarketDressing.SetActive(a);
            if (floristDressing != null) floristDressing.SetActive(!a);
            _item = a ? itemSupermarket : itemFlorist;

            if (itemSupermarket != null) itemSupermarket.available = false;
            if (itemFlorist != null) itemFlorist.available = false;
            if (_item != null) { _item.prompt = a ? "Take the milk and go home" : "Take the flowers and go home"; _item.onInteract.AddListener(() => StartCoroutine(TakeItem())); }
            if (counterBack != null) { counterBack.available = false; counterBack.prompt = "Look behind the counter"; counterBack.onInteract.AddListener(() => StartCoroutine(LookBehindCounter())); }
            if (backroomsOpening != null) backroomsOpening.SetActive(false);
            if (frontDoor != null) frontDoor.Lock();
            if (backroomsDoor != null) { backroomsDoor.Lock(); backroomsDoor.gameObject.SetActive(false); }
            if (shopkeeper != null) { shopkeeper.prompt = "Talk to the shopkeeper"; shopkeeper.onInteract.AddListener(() => StartCoroutine(TalkToShopkeeper())); }
        }

        IEnumerator TalkToShopkeeper()
        {
            var gm = GameManager.Instance;
            gm.narrator.SayAs("Shopkeeper", gm.lines.everythingFree, 3.5f);
            yield return gm.narrator.WaitUntilDone();

            bool done = false, strange = false;
            gm.forkController.RunFork(2, s => { strange = s; done = true; });
            while (!done) yield return null;

            if (strange) { if (counterBack != null) counterBack.available = true; }
            else { if (_item != null) _item.available = true; }
        }

        IEnumerator TakeItem()
        {
            // Carry the item: parent it to the camera.
            var cam = Camera.main;
            if (_item != null && cam != null)
            {
                _item.transform.SetParent(cam.transform, true);
                var t = _item.transform;
                var from = t.localPosition;
                var to = new Vector3(0.3f, -0.3f, 0.6f);
                for (float f = 0f; f < 1f; f += Time.deltaTime * 2f) { t.localPosition = Vector3.Lerp(from, to, f); yield return null; }
                t.localPosition = to;
                foreach (var c in _item.GetComponentsInChildren<Collider>()) c.enabled = false;
            }
            if (frontDoor != null) frontDoor.Unlock();
        }

        IEnumerator LookBehindCounter()
        {
            if (counterPanel != null) counterPanel.SetActive(false);
            if (backroomsOpening != null) backroomsOpening.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            if (backroomsDoor != null) { backroomsDoor.gameObject.SetActive(true); backroomsDoor.Unlock(); }
        }
    }
}
