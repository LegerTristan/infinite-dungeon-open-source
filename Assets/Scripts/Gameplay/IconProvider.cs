using System;
using System.Collections.Generic;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    [Serializable]
    struct FoodIconPair
    {
        public string ThresholdName;
        public Sprite Icon;
    }

    public class IconProvider : MonoBehaviour
    {
        [SerializeField]
        List<FoodIconPair> inspectorFoodIcons = new List<FoodIconPair>();

        static readonly Dictionary<string, Sprite> FoodIconsMap = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        void Awake()
        {
            FoodIconsMap.Clear();
            for (int i = 0; i < inspectorFoodIcons.Count; i++)
            {
                FoodIconPair pair = inspectorFoodIcons[i];
                if (!string.IsNullOrEmpty(pair.ThresholdName) && pair.Icon != null)
                {
                    FoodIconsMap[pair.ThresholdName] = pair.Icon;
                }
            }

            DontDestroyOnLoad(gameObject);
        }

        public static Sprite GetFoodIcon(string satietyGainThreshold)
        {
            if(string.IsNullOrEmpty(satietyGainThreshold))
            {
                return null;
            }

            if (FoodIconsMap.TryGetValue(satietyGainThreshold, out Sprite icon))
            {
                return icon;
            }

            Debug.LogWarning($"[IconProvider] => Could not find a food icon corresponding to key {satietyGainThreshold}");
            return null;
        }
    }
}
