using System;
using System.Collections.Generic;
using UnityEngine;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{

    public class InventoryPanel : BaseComponent
    {
        public event Action<IItem> OnItemSelected = null;

        readonly Dictionary<IItem, ItemBoxPanel> itemBoxes = new Dictionary<IItem, ItemBoxPanel>();

        [SerializeField]
        RectTransform gridContent = null;

        [SerializeField]
        ItemBoxPanel itemBoxPanelPrefab = null;

        ItemBoxPanel selectedItemBox = null;

        protected override bool IsValid() => gridContent && itemBoxPanelPrefab;

        public void AddItemBox(IItem item)
        {
            if(item == null || itemBoxes.ContainsKey(item))
            {
                return;
            }

            ItemBoxPanel newItemBox = Instantiate(itemBoxPanelPrefab, gridContent);
            newItemBox.Initialize(item);
            newItemBox.OnItemBoxSelected += DisplayItemDetail;
            itemBoxes.Add(item, newItemBox);
        }

        void DisplayItemDetail(IItem item)
        {
            if (selectedItemBox)
            {
                selectedItemBox.UnSelectItemBox();
            }

            if (itemBoxes.TryGetValue(item, out ItemBoxPanel itemBox))
            {
                selectedItemBox = itemBox;
                OnItemSelected?.Invoke(item);
            }
        }

        public void DeleteItemBox(IItem item)
        {
            if(item == null)
            {
                return;
            }

            if (itemBoxes.TryGetValue(item, out ItemBoxPanel itemBox))
            {
                if (selectedItemBox == itemBox)
                {
                    selectedItemBox = null;
                }

                itemBox.OnItemBoxSelected -= DisplayItemDetail;
                Destroy(itemBox.gameObject);
                itemBoxes.Remove(item);
            }
        }

        public void DeleteAll()
        {
            foreach (KeyValuePair<IItem, ItemBoxPanel> itemBoxPair in itemBoxes)
            {
                itemBoxPair.Value.OnItemBoxSelected -= DisplayItemDetail;
                Destroy(itemBoxPair.Value.gameObject);
            }

            itemBoxes.Clear();
        }
    }
}