using LingeringHopes.Gameplay;

namespace LingeringHopes.LargeLanguageModel
{
    public struct NarrationStartParameters
    {
        public EnvironmentTag EnvironmentTag { get; }

        public EventTag EventTag { get; }

        public float Satiety { get; }

        public bool PlayerStarving { get; }

        public NarrationStartParameters(EnvironmentTag environmentTag, EventTag eventTag, float satiety, bool playerStarving)
        {
            EnvironmentTag = environmentTag;
            EventTag = eventTag;
            Satiety = satiety;
            PlayerStarving = playerStarving;
        }
    }
}
