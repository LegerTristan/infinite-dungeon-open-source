using System;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class ItemBoxPanel : BaseComponent
    {
        public event Action<IItem> OnItemBoxSelected = null;

        [SerializeField]
        Image imageBox = null;

        [SerializeField]
        Button itemBtn = null;

        [SerializeField]
        Color defaultColor = Color.white, selectedColor = Color.white;

        IItem itemRef = null;

        protected override bool IsValid() => imageBox && itemBtn;

        protected override void Start()
        {
            base.Start();

            itemBtn.onClick.AddListener(SelectItemBox);
            SetButtonNormalColor(defaultColor);
        }

        public void Initialize(IItem item)
        {
            if(item == null)
            {
                return;
            }

            itemRef = item;
            imageBox.sprite = item.Sprite;
        }

        public void UnSelectItemBox()
        {
            SetButtonNormalColor(defaultColor);
        }

        void SelectItemBox()
        {
            SetButtonNormalColor(selectedColor);
            OnItemBoxSelected?.Invoke(itemRef);
        }

        void SetButtonNormalColor(Color color)
        {
            ColorBlock colors = itemBtn.colors;
            colors.normalColor = color;
            colors.selectedColor = color;
            itemBtn.colors = colors;
        }

        void OnDestroy()
        {
            if (itemBtn != null)
            {
                itemBtn.onClick.RemoveListener(SelectItemBox);
            }
        }
    }
}
