using LingeringHopes.Gameplay;
using LingeringHopes.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LingeringHopes.Flow
{
    public abstract class GameMode : BaseComponent
    {
        public event Action<GameState> OnGameModeEnded = null;
        public event Action<SavedDatas> OnGameModeInterrupted = null;

        [Header("Managers")]
        [SerializeField]
        protected SatietyManager satietyManager = null;

        [SerializeField]
        protected ExitManager exitManager = null;

        [SerializeField]
        protected InventoryManager inventoryManager = null;

        [Header("UI")]
        [SerializeField]
        protected CanvasGroup modePanel = null;

        [SerializeField]
        SatietyUI satietyUI = null;

        [SerializeField]
        ExitProgressUI exitProgressUI = null;

        RectTransform modePanelRectTransform = null;

        GameState currentGameState = GameState.ONGOING;

        bool isEnabled = false;

        protected override bool IsValid() => satietyManager && exitManager && inventoryManager && modePanel && satietyUI && exitProgressUI 
            && modePanelRectTransform;

        void Awake()
        {
            if (modePanel != null)
            {
                modePanelRectTransform = modePanel.GetComponent<RectTransform>();
            }
        }

        protected override void Start()
        {
            base.Start();

            satietyManager.OnTiredStackChanged += satietyUI.SetTiredStack;
            satietyManager.OnSatietyChanged += UpdateSatietyUI;
            satietyManager.OnStarveToDeath += LoseGame;

            exitManager.OnExitProgressChanged += UpdateExitProgressUI;
            exitManager.OnExitReached += WinGame;
        }

        public virtual void EnableGameMode()
        {
            gameObject.SetActive(true);
            modePanel.alpha = 1;
            modePanel.interactable = true;
            modePanel.blocksRaycasts = true;
            isEnabled = true;

            satietyUI.UpdateSliderBar(satietyManager.CurrentSatiety, true);
            satietyUI.SetTiredStack(satietyManager.TiredStack);
            exitProgressUI.UpdateSliderBar(exitManager.CurrentProgress, true);

            Canvas.ForceUpdateCanvases();
            if (modePanelRectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(modePanelRectTransform);
            }
        }

        public virtual void RestoreGameModeDatas(SavedDatas datas)
        {
            satietyManager.SetSatiety(datas.Satiety);
            exitManager.SetCurrentProgress(datas.ExitProgress);

            foreach (string itemID in datas.StoredItemIds)
            {
                BaseItem item = Resources.Load<BaseItem>($"Items/{itemID}");
                if (!item)
                    continue;

                inventoryManager.AddItem(Instantiate(item));
            }
        }

        void UpdateSatietyUI(float percentage)
        {
            satietyUI.UpdateSliderBar(percentage, !isEnabled);
        }

        void UpdateExitProgressUI(float percentage)
        {
            exitProgressUI.UpdateSliderBar(percentage, !isEnabled);
        }

        protected virtual void LoseGame()
        {
            currentGameState = GameState.LOSE;
        }

        protected virtual void WinGame()
        {
            currentGameState = GameState.WIN;
        }

        public virtual void DisableGameMode()
        {
            gameObject.SetActive(false);
            modePanel.alpha = 0;
            modePanel.interactable = false;
            modePanel.blocksRaycasts = false;
            isEnabled = false;
        }

        protected void EndGameMode()
        {
            OnGameModeEnded?.Invoke(currentGameState);
        }

        public virtual void RestartGameMode()
        {
            currentGameState = GameState.ONGOING;
            satietyManager.Restart();
            exitManager.Restart();
            inventoryManager.Restart();
        }

        protected abstract void InterruptGameMode();

        protected void CallInteruptGameMode(SavedDatas datas)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            OnGameModeInterrupted?.Invoke(datas);
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                InterruptGameMode();
            }
        }

        void OnApplicationQuit()
        {
            InterruptGameMode();
        }

        protected virtual void OnDestroy()
        {
            if (satietyManager != null)
            {
                satietyManager.OnTiredStackChanged -= satietyUI.SetTiredStack;
                satietyManager.OnSatietyChanged -= UpdateSatietyUI;
                satietyManager.OnStarveToDeath -= LoseGame;
            }

            if (exitManager != null)
            {
                exitManager.OnExitProgressChanged -= UpdateExitProgressUI;
                exitManager.OnExitReached -= WinGame;
            }
        }
    }

}