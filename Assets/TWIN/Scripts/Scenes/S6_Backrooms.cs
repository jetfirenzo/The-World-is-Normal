using TWIN.Player;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 6 (strange): yellow corridors behind the counter. At the end, the street seen from behind:
    /// flat facades held up by beams. The reveal lights switch on when the player reaches the end.
    /// One door leads back to the bedroom (scene 7).
    /// </summary>
    public class S6_Backrooms : MonoBehaviour
    {
        public TriggerZone revealTrigger;
        public GameObject stageSetLights;
        public Door bedroomDoor;

        void Start()
        {
            if (stageSetLights != null) stageSetLights.SetActive(false);
            if (revealTrigger != null) revealTrigger.onPlayerEnter.AddListener(() =>
            {
                if (stageSetLights != null) stageSetLights.SetActive(true);
                Debug.Log("[TWIN] Stage set revealed");
            });
            if (bedroomDoor != null) bedroomDoor.Unlock();
        }
    }
}
