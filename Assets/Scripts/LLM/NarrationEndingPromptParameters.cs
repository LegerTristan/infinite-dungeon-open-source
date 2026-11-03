namespace LingeringHopes.LargeLanguageModel
{
    public class NarrationEndingPromptParameters : PromptParameters
    {
        public readonly ActionResponse UserAction = null;
        public readonly bool RunnedOutOfTime = false;

        public NarrationEndingPromptParameters(ActionResponse response, bool runnedOutOfTime)
        {
            UserAction = response;
            RunnedOutOfTime = runnedOutOfTime;
        }
    }
}