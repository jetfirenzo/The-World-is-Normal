using UnityEngine;

namespace TWIN.Core
{
    /// <summary>
    /// The ordered list of scenes. Edit the asset (Assets/TWIN/Data/SceneFlow.asset) to rename or
    /// reorder scenes; the branch after the shop is decided by fork 3, the ending after the last morning
    /// by EndingResolver (see GameManager.GoToNextScene).
    /// </summary>
    [CreateAssetMenu(menuName = "TWIN/Scene Flow", fileName = "SceneFlow")]
    public class SceneFlow : ScriptableObject
    {
        public string boot = "00_Boot";
        public string bedroom = "01_Bedroom";
        public string bathroom = "02_Bathroom";
        public string livingRoom = "03_LivingRoom";
        public string street = "04_Street";
        public string shop = "05_Shop";
        public string frontDoor = "06A_FrontDoor";
        public string backrooms = "06B_Backrooms";
        public string lastMorning = "07_LastMorning";
        public string endOfDemo = "99_EndOfDemo";

        [Header("Endings (loaded additively on top of the last morning)")]
        public string endGoodMorningSam = "End_GoodMorningSam";
        public string endTheNote = "End_TheNote";
        public string endTheFuneral = "End_TheFuneral";
        public string endWakeUp = "End_WakeUp";
        public string endSystemError = "End_SystemError";

        /// <summary>Every scene in play order (branch scenes and endings all listed), for the Build Settings.</summary>
        public string[] All => new[]
        {
            boot, bedroom, bathroom, livingRoom, street, shop, frontDoor, backrooms, lastMorning, endOfDemo,
            endGoodMorningSam, endTheNote, endTheFuneral, endWakeUp, endSystemError
        };

        public string EndingScene(Ending ending)
        {
            switch (ending)
            {
                case Ending.GoodMorningSam: return endGoodMorningSam;
                case Ending.TheNote: return endTheNote;
                case Ending.TheFuneral: return endTheFuneral;
                case Ending.WakeUp: return endWakeUp;
                case Ending.SystemError: return endSystemError;
                default: return endOfDemo;
            }
        }

        /// <summary>
        /// The scene that follows <paramref name="current"/>, given the state. Null at the end.
        /// After the last morning this returns the END OF DEMO fallback; GameManager decides whether to
        /// load the resolved ending instead.
        /// </summary>
        public string NextAfter(string current, GameState state)
        {
            if (current == boot) return bedroom;
            if (current == bedroom) return bathroom;
            if (current == bathroom) return livingRoom;
            if (current == livingRoom) return street;
            if (current == street) return shop;
            if (current == shop) return state.forkWasStrange[2] ? backrooms : frontDoor;
            if (current == frontDoor) return lastMorning;
            if (current == backrooms) return lastMorning;
            if (current == lastMorning) return endOfDemo;
            if (current == endOfDemo) return bedroom;
            return null;
        }
    }
}
