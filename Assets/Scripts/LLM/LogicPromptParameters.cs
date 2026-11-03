namespace LingeringHopes.LargeLanguageModel
{
    public class LogicPromptParameters : PromptParameters
    {
        public readonly string EventNarration = null;

        public readonly string EventHint = null;

        public readonly int NbrActions = 1;

        public LogicPromptParameters(string narration, string hint, int nbrActions)
        {
            EventNarration = narration;
            EventHint = hint;
            NbrActions = nbrActions;
        }
    }
}

