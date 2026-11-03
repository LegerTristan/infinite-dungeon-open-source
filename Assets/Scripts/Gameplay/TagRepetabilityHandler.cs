using System.Collections.Generic;

namespace LingeringHopes.Gameplay
{
    public class TagRepetabilityHandler
    {
        readonly Dictionary<BaseTag, int> usedTags = new Dictionary<BaseTag, int>();
        readonly List<BaseTag> tagsToRemove = new List<BaseTag>();

        public void AddUsedTag(BaseTag tag)
        {
            if (tag == null)
                return;

            usedTags.Add(tag, tag.RepeatDelay);
        }

        public void UpdateUsedTags()
        {
            tagsToRemove.Clear();
            tagsToRemove.AddRange(usedTags.Keys);

            for (int i = 0; i < tagsToRemove.Count; i++)
            {
                BaseTag tag = tagsToRemove[i];
                int remainingTurns = usedTags[tag] - 1;

                if (remainingTurns <= 0)
                {
                    usedTags.Remove(tag);
                }
                else
                {
                    usedTags[tag] = remainingTurns;
                }
            }

            for (int i = 0; i < tagsToRemove.Count; i++)
            {
                usedTags.Remove(tagsToRemove[i]);
            }
        }

        public bool IsTagAvailable(BaseTag tag)
        {
            return tag != null && !usedTags.ContainsKey(tag);
        }

        public void Clear()
        {
            usedTags.Clear();
            tagsToRemove.Clear();
        }
    }

}