using UnityEngine;

namespace LingeringHopes.Gameplay
{
    [CreateAssetMenu(fileName = "FoodItem", menuName = "Item/Food")]
    public class FoodItem : BaseItem
    {
        [SerializeField, Range(0f, 1f)]
        float satietyGain = 0.2f;

        public float SatietyGain => satietyGain;

        public void Initialize(string name, string description, Sprite itemIcon, float satiety)
        {
            itemName = name;
            itemDescription = description;
            itemSprite = itemIcon;
            satietyGain = satiety;
        }

        public override void Use(ItemUseContext context)
        {
            context.SatietyManager.IncreaseSatiety(satietyGain);
        }
    }

}
