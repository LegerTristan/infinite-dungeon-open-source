using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class ItemFactory : BaseComponent
    {
        [Header("Gatherable Items during Rest Phase")]
        [SerializeField] 
        FoodItem[] gatherableFoods = null;

        protected override bool IsValid() => gatherableFoods != null && gatherableFoods.Length > 0;

        /// <summary>
        /// Create an item based on the predefined list of gatherable items.
        /// </summary>
        /// <returns>An item from the gatherable list</returns>
        public IItem CreateItem()
        {
            if (!IsValid())
            {
                return null;
            }

            return gatherableFoods[Random.Range(0, gatherableFoods.Length)];
        }

        /// <summary>
        /// Create a copy of a random predefined item.
        /// </summary>
        public IItem CreateRandomGatherableItem()
        {
            if (gatherableFoods == null || gatherableFoods.Length == 0) return null;

            FoodItem prefab = gatherableFoods[Random.Range(0, gatherableFoods.Length)];
            return prefab != null ? Instantiate(prefab) : null;
        }

        /// <summary>
        /// Create a new food based on the provided information.
        /// </summary>
        public FoodItem CreateDynamicFood(string itemName, string itemDescription, string satietyGainTier)
        {
            FoodItem dynamicFood = ScriptableObject.CreateInstance<FoodItem>();

            float satietyGain = ResolveSatietyGain(satietyGainTier);
            Sprite itemIcon = IconProvider.GetFoodIcon(satietyGainTier);

            dynamicFood.Initialize(itemName, itemDescription, itemIcon, satietyGain);
            return dynamicFood;
        }

        private float ResolveSatietyGain(string tier)
        {
            return tier switch
            {
                "MEDIUM" => Random.Range(0.1f, 0.2f),
                "HIGH" => Random.Range(0.2f, 0.4f),
                _ => Random.Range(0f, 0.1f) 
            };
        }
    }
}