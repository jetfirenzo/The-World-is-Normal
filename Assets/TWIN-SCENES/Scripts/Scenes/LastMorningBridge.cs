using System.Collections;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Lives in 07_LastMorning. The ending scenes are loaded additively on top of it and reach its
    /// bed, door and north wall through this component (EndingController finds it at runtime).
    /// </summary>
    public class LastMorningBridge : MonoBehaviour
    {
        [Tooltip("Where the player lies down: position = head, forward = looking at the ceiling")]
        public Transform bedLiePoint;
        [Tooltip("The door that appeared in the wall")]
        public Transform appearedDoor;
        [Tooltip("The solid north wall; an ending replaces it with its own wall-with-doorway")]
        public GameObject northWall;
        [Tooltip("The whole wrong bedroom (hidden by endings that show their own room)")]
        public GameObject bedroomRoot;
        [Tooltip("The note in the player's hand (parented to the camera); endings hide it")]
        public GameObject handNote;

        /// <summary>Swing the appeared door open around its hinge (left edge) and remove the solid wall behind it.</summary>
        public IEnumerator OpenDoor(float seconds)
        {
            if (northWall != null) northWall.SetActive(false);
            if (appearedDoor == null) yield break;
            // Rotate around the hinge edge: move the pivot to the left edge of the slab.
            Vector3 hinge = appearedDoor.position - appearedDoor.right * (appearedDoor.lossyScale.x * 0.5f);
            float done = 0f;
            float target = -100f;
            while (done > target)
            {
                float step = Mathf.Max(target - done, -Mathf.Abs(target) * Time.deltaTime / Mathf.Max(0.01f, seconds));
                appearedDoor.RotateAround(hinge, Vector3.up, step);
                done += step;
                yield return null;
            }
        }
    }
}
