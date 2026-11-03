using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    [Serializable]
    public class SavedDatas
    {
        [SerializeField]
        string[] storedItemIds = null;

        [SerializeField]
        float satiety = 1.0f;

        [SerializeField]
        float exitProgress = 0.0f;

        [SerializeField]
        bool isInEvent = false;

        public string[] StoredItemIds => storedItemIds;

        public float Satiety => satiety;

        public float ExitProgress => exitProgress;

        public bool IsInEvent => isInEvent;
    
        public SavedDatas(IReadOnlyList<IItem> items, float satiety, float exitProgress, bool inEvent) 
        {
            this.satiety = satiety;
            this.exitProgress = exitProgress;
            isInEvent = inEvent;

            if(items == null)
            {
                storedItemIds = Array.Empty<string>();
                return;
            }

            List<string> ids = new List<string>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is BaseItem baseItem && !string.IsNullOrEmpty(baseItem.ID))
                {
                    ids.Add(baseItem.ID);
                }
            }
            storedItemIds = ids.ToArray();
        }
    }
}