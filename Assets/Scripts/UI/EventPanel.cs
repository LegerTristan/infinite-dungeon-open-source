using LingeringHopes.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LingeringHopes.UI
{
    public class EventPanel : BaseComponent
    {
        public event Action<int> OnUserActionSelected = null;
        public event Action OnConfirmClicked = null;
        public event Action OnRelaunchLLMProcessClicked = null;

        List<EventActionButton> actionButtons = new List<EventActionButton>();

        [Header("Chat")]
        [SerializeField]
        ChatHistory chatHistory = null;

        [Header("Event Components")]
        [SerializeField]
        Button confirmButton = null, relaunchButton = null;

        [SerializeField]
        RectTransform actionButtonContainer = null;

        [SerializeField]
        EventActionButton actionButtonPrefab = null;

        [Header("Consequence Panels")]
        [SerializeField]
        ConsequencePanel satietyPanel = null, progressionPanel = null, gainedObjectPanel = null;

        int displayedActionCount = 0;

        protected override bool IsValid() => actionButtonPrefab && actionButtonContainer && chatHistory && confirmButton
            && satietyPanel && progressionPanel && gainedObjectPanel && relaunchButton;

        public void Initialize()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            relaunchButton.onClick.AddListener(OnRelaunchClicked);

            ToggleRelaunchButton(false);
        }

        void CreateActionButton()
        {
            EventActionButton newActionButton = Instantiate(actionButtonPrefab, actionButtonContainer);
            newActionButton.OnActionCalled += () => OnActionChoose(newActionButton);
            actionButtons.Add(newActionButton);
            newActionButton.SetActionButtonState(false);
            newActionButton.HideAction();
        }

        public int CreateActions(int numberActions)
        {
            while (actionButtons.Count < numberActions)
            {
                CreateActionButton();
            }

            for (int i = 0; i < actionButtons.Count; ++i)
            {
                bool shouldBeActive = i < numberActions;
                actionButtons[i].gameObject.SetActive(shouldBeActive);
            }

            return numberActions;
        }

        public int AddAction()
        {
            if (displayedActionCount >= actionButtons.Count)
            {
                CreateActionButton();
            }

            actionButtons[displayedActionCount].DisplayAction();
            displayedActionCount++;

            return displayedActionCount;
        }

        public void AddTextToAction(string actionText, int actionIndex)
        {
            if (actionIndex >= 0 && actionIndex < actionButtons.Count)
            {
                actionButtons[actionIndex].SetActionText(actionText);
            }
        }

        public void SetActionButtonState(bool state)
        {
            foreach (EventActionButton button in actionButtons)
            {
                button.SetActionButtonState(state);
            }
        }

        public void HideActions()
        {
            foreach (EventActionButton button in actionButtons)
            {
                button.HideAction();
            }

            displayedActionCount = 0;
        }

        void OnActionChoose(EventActionButton actionButton)
        {
            OnUserActionSelected?.Invoke(actionButtons.IndexOf(actionButton));
        }

        void OnConfirmButtonClicked()
        {
            ToggleConfirmButton(false);
            OnConfirmClicked?.Invoke();
        }

        public void DisplayStreamingToken(string token) => chatHistory.AddTextToHistory(token, false);

        public void AddTextToHistory(string text) => chatHistory.AddTextToHistory(text, true);

        public void AddPlaceholderTextToHistory(string placeholder = "Narrator is thinking...") => chatHistory.AddPlaceholderTextToHistory($"<color=#A0A0A0><i>{placeholder}</i></color>");

        public void AddErrorTextToHistory(string error = "An error has occured.") => chatHistory.AddPlaceholderTextToHistory($"<color=red><i>{error}</i></color>");
        
        public void ToggleConfirmButton(bool visible)
        {
            confirmButton.interactable = visible;
            confirmButton.gameObject.SetActive(visible);
        }

        public void ToggleRelaunchButton(bool visible)
        {
            relaunchButton.interactable = visible;
            relaunchButton.gameObject.SetActive(visible);
        }

        public void ShowSatietyConsequence(string message, Color color) => satietyPanel.ShowConsequence(message, color);
        public void ShowProgressionConsequence(string message, Color color) => progressionPanel.ShowConsequence(message, color);
        public void ShowItemConsequence(string message, Color color) => gainedObjectPanel.ShowConsequence(message, color);

        public void OnRelaunchClicked()
        {
            ToggleRelaunchButton(false);
            OnRelaunchLLMProcessClicked?.Invoke();
        }

        public void Clear()
        {
            chatHistory.Clear();
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            }

            if (relaunchButton != null)
            {
                relaunchButton.onClick.RemoveListener(OnRelaunchClicked);
            }
        }
    }

}
