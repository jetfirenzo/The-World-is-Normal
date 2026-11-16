using UnityEngine;

namespace TWIN.Narration
{
    /// <summary>
    /// Every spoken or written line in the demo. Edit Assets/TWIN/Data/NarratorLines.asset,
    /// no code change needed. Scenes read them via GameManager.Instance.lines.
    /// </summary>
    [CreateAssetMenu(menuName = "TWIN/Narrator Lines", fileName = "NarratorLines")]
    public class NarratorLines : ScriptableObject
    {
        [Header("Scene 1 - Bedroom")]
        public string goodMorning = "Good morning, Sam.";

        [Header("Scene 2 - Bathroom")]
        public string noteText = "Your name isn't Sam.";

        [Header("Scene 3 - Living room")]
        public string breakfast = "Breakfast is the most important meal of the day.";
        public string outOfMilk = "We're out of milk. Let's go to the shop.";
        public string phoneVoice = "Your mom died this morning.";
        public string howSad = "How sad. Let's get her some flowers.";

        [Header("Scene 5 - Shop")]
        public string missingPoster = "HAVE YOU SEEN THIS MAN?";
        public string bouquetCard = "In loving memory of your mom.";
        public string everythingFree = "That's free. Everything is free today.";

        [Header("Scene 7 - Last morning")]
        public string noteTextLastMorning = "Don't go back to sleep.";
        public string backToSleep = "Time to go back to sleep, Sam.";

        [Header("End of demo")]
        public string endOfDemo = "END OF DEMO";
    }
}
