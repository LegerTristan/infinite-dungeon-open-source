namespace LingeringHopes.LargeLanguageModel
{
    public class NarrationContinuePromptParameters : PromptParameters
    {
        public readonly ActionResponse UserAction = null;
        public readonly string SatietyChange = null;
        public readonly string ProgressionChange = null;
        public readonly bool IsRunningOutOfTime = false;

        public NarrationContinuePromptParameters(ActionResponse response, string satietyChange, string progressionChange, bool runningOutOfTime)
        {
            UserAction = response;
            SatietyChange = satietyChange;
            ProgressionChange = progressionChange;
            IsRunningOutOfTime = runningOutOfTime;
        }
    }
}

