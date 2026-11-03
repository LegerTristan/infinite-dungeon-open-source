using System;

namespace LingeringHopes.LargeLanguageModel
{
    [Serializable]
    public class NarrationResponse
    {
        public string narration;
        public string summary;
    }

    [Serializable]
    public class LogicResponse
    {
        public string satiety = "NO CHANGE";
        public string progression = "NO CHANGE";
        public string itemCollected = "NONE";
    }

    [Serializable]
    public class ActionResponse : LogicResponse
    {
        public string action = string.Empty;
        public string ended = "NO";

        public override string ToString()
        {
            return $"Action: {action}, Satiety: {satiety}, Progression: {progression}, ItemCollected: {itemCollected}, Ended: {ended}";
        }
    }

    [Serializable]
    public class ItemResponse
    {
        public string itemDescription = null;
        public string satietyGain = null;
        public string type = null;
    }
}