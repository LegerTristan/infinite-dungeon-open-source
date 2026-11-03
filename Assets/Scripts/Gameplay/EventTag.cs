using UnityEngine;

namespace LingeringHopes.Gameplay
{
    [CreateAssetMenu(fileName = "EventTag", menuName = "Tags/EventTag", order = 2)]
    public class EventTag : BaseTag
    {
        [SerializeField]
        string[] hints = null;

        [SerializeField, Range(1, 5)]
        int durationBeforeEnded = 1, maxDuration = 1;

        public string Hint
        {
            get
            {
                if (hints == null || hints.Length == 0)
                {
                    Debug.LogError($"[EventTag] \"{name}\" has no hints defined!");
                    return string.Empty;
                }
                return hints[Random.Range(0, hints.Length)];
            }
        }

        public int MaxDuration => maxDuration;

        public int EndedDelay => durationBeforeEnded;

        protected override TagType TagType => TagType.EVENT;
    }
}
