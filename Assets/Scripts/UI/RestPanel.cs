using System;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{

    public class RestPanel : BaseComponent
    {
        public event Action OnSleepCalled = null,
                            OnGatherCalled = null,
                            OnProgressCalled = null;

        [SerializeField]
        Button sleepButton = null, gatherButton = null, progressButton = null;

        protected override bool IsValid() => sleepButton && gatherButton && progressButton;

        protected override void Start()
        {
            base.Start();

            sleepButton.onClick.AddListener(OnSleepButtonClicked);
            gatherButton.onClick.AddListener(OnGatherButtonClicked);
            progressButton.onClick.AddListener(OnProgressButtonClicked);
        }

        void OnEnable()
        {
            SetButtonsState(true);
        }

        void OnSleepButtonClicked()
        {
            OnSleepCalled?.Invoke();
        }

        void OnGatherButtonClicked()
        {
            OnGatherCalled?.Invoke();
        }

        void OnProgressButtonClicked()
        {
            OnProgressCalled?.Invoke();
        }

        public void SetButtonsState(bool state)
        {
            sleepButton.interactable = state;
            progressButton.interactable = state;
            gatherButton.interactable = state;
        }

        void OnDisable()
        {
            SetButtonsState(false);
        }
        
        void OnDestroy()
        {
            if (sleepButton != null)
            {
                sleepButton.onClick.RemoveListener(OnSleepButtonClicked);
            }

            if (gatherButton != null)
            {
                gatherButton.onClick.RemoveListener(OnGatherButtonClicked);
            }

            if (progressButton != null)
            {
                progressButton.onClick.RemoveListener(OnProgressButtonClicked);
            }
        }
    }
}