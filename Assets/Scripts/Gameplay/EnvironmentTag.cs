using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LingeringHopes.Gameplay
{
    [CreateAssetMenu(fileName = "EnvironmentTag", menuName = "Tags/EnvironmentTag", order = 1)]
    public class EnvironmentTag : BaseTag
    {
        [SerializeField]
        List<EventTag> compatibleEvents = new List<EventTag>();

        [SerializeField]
        List<string> decorationInformation = new List<string>();

        protected override TagType TagType => TagType.ENVIRONMENT;

        public string GetRandomDecorationInformation()
        {
            if (decorationInformation == null || decorationInformation.Count == 0)
            {
                return string.Empty;
            }
            return decorationInformation[Random.Range(0, decorationInformation.Count)];
        }

        public EventTag GetRandomCompatibleEvent(Predicate<BaseTag> subCondition = null)
        {
            if (compatibleEvents == null || compatibleEvents.Count == 0)
            {
                return null;
            }

            if (subCondition == null)
            {
                return compatibleEvents[Random.Range(0, compatibleEvents.Count)];
            }

            List<EventTag> validEvents = new List<EventTag>();
            for (int i = 0; i < compatibleEvents.Count; i++)
            {
                EventTag evt = compatibleEvents[i];
                if (evt != null && subCondition(evt))
                {
                    validEvents.Add(evt);
                }
            }

            if (validEvents.Count == 0)
                return null;

            return validEvents[Random.Range(0, validEvents.Count)];
        }
    }
}