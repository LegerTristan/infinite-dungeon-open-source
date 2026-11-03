using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LingeringHopes.Gameplay
{
    [CreateAssetMenu(fileName = "TagLibrary", menuName = "Tags/TagLibrary", order = 1)]
    public class TagLibrary : ScriptableObject
    {
        [SerializeField]
        EnvironmentTag[] envTags = null;

        public bool ContainsTag(TagType type, string tagName)
        {
            return type switch
            {
                TagType.ENVIRONMENT => ContainsTag(envTags, tagName),
                _ => LogUnsupportedType(type)
            };
        }

        bool LogUnsupportedType(TagType type)
        {
            Debug.LogError($"[TagLibrary] Tag type {type} is not supported.");
            return false;
        }

        bool ContainsTag(BaseTag[] tags, string tagName)
        {
            if(tags == null)
            {
                return false;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] != null && string.Equals(tags[i].TagName, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            if (envTags != null)
            {
                for (int i = 0; i < envTags.Length; i++)
                    if (envTags[i] != null) result.AppendLine(envTags[i].ToString());
            }

            return result.ToString();
        }

        public T GetRandomTagOfType<T>(TagType type, Predicate<BaseTag> subCondition = null) where T : BaseTag
        {
            return type switch
            {
                TagType.ENVIRONMENT => GetRandomTagFromArray(envTags, subCondition) as T,
                _ => null
            };
        }

        BaseTag GetRandomTagFromArray(BaseTag[] tags, Predicate<BaseTag> subCondition = null)
        {
            if (tags == null || tags.Length == 0) return null;

            if (subCondition == null)
            {
                return tags[Random.Range(0, tags.Length)];
            }

            List<BaseTag> validTags = new List<BaseTag>();
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] != null && subCondition(tags[i]))
                {
                    validTags.Add(tags[i]);
                }
            }

            if (validTags.Count == 0)
            {
                return null;
            }

            return validTags[Random.Range(0, validTags.Count)];
        }

        public List<T> GetAllTagsOfType<T>(TagType type, bool randomize = false) where T : BaseTag
        {
            BaseTag[] sourceArray = null;

            switch (type)
            {
                case TagType.ENVIRONMENT:
                    sourceArray = envTags;
                    break;
                default:
                    return null;
            }

            if (sourceArray == null)
            {
                return new List<T>();
            }

            List<BaseTag> listTags = new List<BaseTag>(sourceArray);
            if (randomize)
            {
                listTags.Shuffle();
            }

            List<T> result = new List<T>(listTags.Count);
            for (int i = 0; i < listTags.Count; i++)
            {
                if (listTags[i] is T typedTag)
                {
                    result.Add(typedTag);
                }
            }

            return result;
        }
    }
}