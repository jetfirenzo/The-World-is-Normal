using TWIN.Player;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Scene 4: identical houses, every door number 9. A ball hits the player in the back (BallFromBehind,
    /// started by a trigger halfway down the street). A child at the end only moves when not looked at (WeepingChild).
    /// The shop door at the end leads to scene 5.
    /// </summary>
    public class S4_Street : MonoBehaviour
    {
        public TriggerZone ballTrigger;
        public BallFromBehind ball;
        public WeepingChild child;
        public Door shopDoor;

        void Start()
        {
            if (ballTrigger != null && ball != null) ballTrigger.onPlayerEnter.AddListener(ball.Launch);
            if (shopDoor != null) shopDoor.Unlock();
        }
    }
}
