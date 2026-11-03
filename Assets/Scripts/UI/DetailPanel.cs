using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class DetailPanel : BaseComponent
    {
        public event Action<IItem> OnItemAskedForConsumption = null;

        RestAction actionRef = null;

        IItem itemRef = null;

        [Header("UI References")]
        [SerializeField]
        Button actionBtn = null;

        [SerializeField]
        TMP_Text nameText = null, descriptionText = null, actionBtnText = null;

        protected override bool IsValid() => nameText && descriptionText && actionBtn && actionBtnText;

        protected override void Start()
        {
            base.Start();

            actionBtn.onClick.AddListener(ExecuteAction);
            actionBtn.gameObject.SetActive(false);

            ClearDetails();
        }

        void OnEnable()
        {
            if (actionBtn != null)
                actionBtn.interactable = true;
        }

        void ExecuteAction()
        {
            if (itemRef != null)
            {
                actionBtn.interactable = false;
                OnItemAskedForConsumption?.Invoke(itemRef);
            }
            else if (actionRef != null)
            {
                actionBtn.interactable = false;
                actionRef.Execute();
            }
        }

        public void DisplayItemDetail(IItem item)
        {
            if(item == null)
            {
                return;
            }

            actionRef = null;
            itemRef = item;

            nameText.text = item.Name;
            descriptionText.text = item.Description;
            actionBtnText.text = "Consume";

            actionBtn.interactable = true;
            actionBtn.gameObject.SetActive(true);
        }

        public void DisplayActionDetail(RestAction action)
        {
            if(action == null)
            {
                return;
            }

            actionRef = action;
            itemRef = null;

            nameText.text = action.ActionName;
            descriptionText.text = action.ActionDescription;
            actionBtnText.text = "Execute";

            actionBtn.interactable = true;
            actionBtn.gameObject.SetActive(true);
        }

        public void ClearDetails()
        {
            itemRef = null;
            actionRef = null;
            nameText.text = string.Empty;
            descriptionText.text = string.Empty;
            actionBtn.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            actionBtn.interactable = false;
        }

        private void OnDestroy()
        {
            if (actionBtn != null)
            {
                actionBtn.onClick.RemoveListener(ExecuteAction);
            }
        }
    }

}