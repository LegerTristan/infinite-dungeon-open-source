using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class EndGamePanel : BaseComponent
    {
        public event Action OnRestartCalled = null;

        [Header("Components")]
        [SerializeField]
        RectTransform panel = null;

        [SerializeField]
        TMP_Text reasonText = null;

        [SerializeField]
        Button restartButton = null;

        [Header("End Game Messages")]
        [SerializeField]
        string winReason = "You managed to found an exit and escaped the dungeon.\nCongratulations !",
               loseReason = "You could not escape the dungeon. You starved to death...";

        protected override bool IsValid() => panel && reasonText && restartButton;

        protected override void Start()
        {
            base.Start();

            restartButton.onClick.AddListener(OnRestartButtonClicked);
            Hide();
        }

        public void ShowWinEndGame()
        {
            ShowEndGame(winReason);
        }

        public void ShowLoseEndGame()
        {
            ShowEndGame(loseReason);
        }

        void ShowEndGame(string reason)
        {
            reasonText.text = reason;
            panel.localScale = Vector3.one;
        }

        void OnRestartButtonClicked()
        {
            OnRestartCalled?.Invoke();
        }

        public void Hide()
        {
            if(panel)
            {
                panel.localScale = Vector3.zero;
            }
        }

        void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }
        }
    }

}