using System;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    [Serializable]
    public abstract class BaseItem : ScriptableObject, IItem
    {
        [SerializeField]
        string itemID = "Default";

        [SerializeField]
        protected string itemName = "Food";

        [SerializeField, TextArea]
        protected string itemDescription = "Good to eat.";

        [SerializeField]
        protected Sprite itemSprite = null;

        [SerializeField]
        protected ItemType itemType = ItemType.FOOD;

        public Sprite Sprite => itemSprite;
        public string ID => itemID;
        public string Name => itemName;
        public string Description => itemDescription;
        public ItemType Type => itemType;

        public abstract void Use(ItemUseContext context);
    }
}