using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LingeringHopes.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuPanel : FadeablePanel
    {
        public event Action OnNewGameAsked = null,
                            OnContinueGameAsked = null,
                            OnQuitGameAsked = null;

        [SerializeField]
        VideoPlayer videoPlayer = null;

        [SerializeField]
        Button newGameButton = null, continueButton = null, quitGameButton = null;

        bool enableContinueButton = false;

        protected override bool IsValid() => newGameButton && continueButton && quitGameButton && videoPlayer;

        protected override void Start()
        {
            base.Start();

            newGameButton.onClick.AddListener(OnNewGameCalled);
            continueButton.onClick.AddListener(OnContinueGameCalled);
            quitGameButton.onClick.AddListener(OnQuitGameCalled);
        }

        void OnNewGameCalled()
        {
            OnNewGameAsked?.Invoke();
        }

        void OnContinueGameCalled()
        {
            OnContinueGameAsked?.Invoke();
        }

        void OnQuitGameCalled()
        {
            OnQuitGameAsked?.Invoke();
        }

        void SetButtonState(bool enabled)
        {
            newGameButton.interactable = enabled;
            continueButton.interactable = enabled && enableContinueButton;
            quitGameButton.interactable = enabled;
        }

        public void Display(bool canContinue = false)
        {
            enableContinueButton = canContinue;

            // Order matters here. Ensure enableContinueButton is updated first.
            SetButtonState(true);
            ShowFade();
        }

        public void Close()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            SetButtonState(false);
            HideFade(() => gameObject.SetActive(false));
        }

        void OnDestroy()
        {
            if(!IsValid())
            {
                return;
            }

            newGameButton.onClick.RemoveListener(OnNewGameCalled);
            continueButton.onClick.RemoveListener(OnContinueGameCalled);
            quitGameButton.onClick.RemoveListener(OnQuitGameCalled);
        }
    }
}
