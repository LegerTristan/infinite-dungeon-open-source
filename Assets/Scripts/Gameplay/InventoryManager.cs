using System;
using System.Collections.Generic;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class InventoryManager : MonoBehaviour
    {
        public event Action<IItem> OnItemAdded = null,
                                   OnItemRemoved = null;
        public event Action OnItemsCleared = null;

        readonly List<IItem> items = new List<IItem>();

        [SerializeField, Header("Debug")]
        List<FoodItem> startingFoodItems = new List<FoodItem>();

        public IReadOnlyList<IItem> Items => items;

        void Start()
        {
            AddStartingItems();
        }

        void AddStartingItems()
        {
            // Add starting food items for debugging
            foreach (FoodItem foodItem in startingFoodItems)
            {
                AddItem(Instantiate(foodItem));
            }
        }

        public void AddItem(IItem item)
        {
            if (item == null)
                return;

            items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void ConsumeItem(IItem item, ItemUseContext useContext)
        {
            if (items.Contains(item))
            {
                item.Use(useContext);
                RemoveItem(item);
            }
        }

        public void RemoveItem(IItem item)
        {
            if(items.Remove(item))
            {
                OnItemRemoved?.Invoke(item);
            }
        }

        public void Restart()
        {
            items.Clear();
            AddStartingItems();
        }
    }

}