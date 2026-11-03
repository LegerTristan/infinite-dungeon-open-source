using LingeringHopes.Gameplay;

namespace LingeringHopes.LargeLanguageModel
{
    public class NarrationStartPromptParameters : PromptParameters
    {
        public readonly EnvironmentTag EnvironmentTag = null;

        public readonly EventTag EventTag = null;

        public readonly float Satiety = 1f;

        public readonly bool PlayerStarving = false;

        public NarrationStartPromptParameters(EnvironmentTag envTag, EventTag evtTag, float satiety, bool isPlayerStarving)
        {
            EnvironmentTag = envTag;
            EventTag = evtTag;
            Satiety = satiety;
            PlayerStarving = isPlayerStarving;
        }
    }

}
