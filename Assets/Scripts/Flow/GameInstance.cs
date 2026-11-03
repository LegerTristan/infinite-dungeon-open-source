using LingeringHopes.UI;
using System;
using System.Collections;
using UnityEngine;
using LingeringHopes.Gameplay;

namespace LingeringHopes.Flow
{
    public class GameInstance : BaseComponent
    {
        #region Fields
        [Header("Managers")]
        [SerializeField]
        DownloadManager downloadManager = null;

        [SerializeField]
        IntroManager introManager = null;

        [SerializeField]
        SaveManager saveManager = null;

        [Header("GameModes")]
        [SerializeField]
        EventMode eventMode = null;

        [SerializeField]
        RestMode restMode = null;

        [Header("UI Panels")]
        [SerializeField]
        MenuPanel menuPanel = null;

        [SerializeField]
        FadeablePanel changePhaseFadePanel = null;

        [SerializeField]
        EndGamePanel endGamePanel = null;

        [Header("Animations")]
        [SerializeField, Range(1f, 10f)]
        float modeTransitionDuration = 3f;
        #endregion
        protected override bool IsValid() => downloadManager && introManager && saveManager && eventMode && restMode && menuPanel &&
            changePhaseFadePanel && endGamePanel;

        protected override async void Start()
        {
            base.Start();

            Application.targetFrameRate = 30;   // Limit to 30FPS

            eventMode.OnGameModeEnded += OnEventModeEnded;
            eventMode.OnGameModeInterrupted += SaveGameModeDatas;
            eventMode.DisableGameMode();

            restMode.OnGameModeEnded += OnRestModeEnded;
            restMode.OnGameModeInterrupted += SaveGameModeDatas;
            restMode.DisableGameMode();

            menuPanel.OnNewGameAsked += LaunchGame;
            menuPanel.OnContinueGameAsked += ContinueGame;
            menuPanel.OnQuitGameAsked += Application.Quit;

            endGamePanel.OnRestartCalled += RestartGame;

            menuPanel.gameObject.SetActive(false);
            bool hasDownloaded = await downloadManager.DownloadAssets();
            if (!hasDownloaded)
            {
                return;
            }

            menuPanel.gameObject.SetActive(true);
            menuPanel.Display(saveManager.ContainsSave());
        }

        async void LaunchGame()
        {
            menuPanel.Close();

            downloadManager.gameObject.SetActive(false);
            introManager.gameObject.SetActive(true);
            await introManager.InitializeGame();
            introManager.gameObject.SetActive(false);
            menuPanel.gameObject.SetActive(false);

            SwitchToRestMode();
            changePhaseFadePanel.HideFade();
        }

        void ContinueGame()
        {
            menuPanel.Close();
            downloadManager.gameObject.SetActive(false);

            // Load datas from save
            SavedDatas datas = saveManager.Load();
            if (datas != null)
            {
                if (datas.IsInEvent)
                {
                    eventMode.RestoreGameModeDatas(datas);
                    OnRestModeEnded(GameState.ONGOING);
                }
                else
                {
                    restMode.RestoreGameModeDatas(datas);
                    OnEventModeEnded(GameState.ONGOING);
                }
            }
            else
            {
                // Launch the default game mode after initialization if we have no save.
                OnEventModeEnded(GameState.ONGOING);
            }
        }

        void SaveGameModeDatas(SavedDatas datas)
        {
            saveManager.Save(datas);
        }

        void HandleGameModeEnded(GameState state, Action changeModeCallback)
        {
            switch (state)
            {
                case GameState.ONGOING:
                default:
                    StartCoroutine(FadeAnimation(changeModeCallback));
                    break;
                case GameState.LOSE:
                    restMode.DisableGameMode();
                    eventMode.DisableGameMode();
                    StartCoroutine(FadeAnimation(endGamePanel.ShowLoseEndGame));
                    break;
                case GameState.WIN:
                    restMode.DisableGameMode();
                    eventMode.DisableGameMode();
                    StartCoroutine(FadeAnimation(endGamePanel.ShowWinEndGame));
                    break;
            }
        }

        void OnRestModeEnded(GameState state)
        {
            HandleGameModeEnded(state, SwitchToEventMode);
        }

        void SwitchToEventMode()
        {
            restMode.DisableGameMode();
            restMode.gameObject.SetActive(false);
            downloadManager.gameObject.SetActive(false);
            introManager.gameObject.SetActive(false);
            saveManager.gameObject.SetActive(false);

            eventMode.EnableGameMode();
            eventMode.gameObject.SetActive(true);
        }

        void OnEventModeEnded(GameState state) => HandleGameModeEnded(state, SwitchToRestMode);

        void SwitchToRestMode()
        {
            eventMode.DisableGameMode();
            eventMode.gameObject.SetActive(false);
            eventMode.PrepareEvent();

            restMode.EnableGameMode();
            restMode.gameObject.SetActive(true);
        }

        IEnumerator FadeAnimation(Action callback)
        {
            changePhaseFadePanel.gameObject.SetActive(true);
            yield return changePhaseFadePanel.ShowFadeCoroutine();
            yield return new WaitForSeconds(modeTransitionDuration);

            callback?.Invoke();

            yield return changePhaseFadePanel.HideFadeCoroutine();
            changePhaseFadePanel.gameObject.SetActive(false);
        }

        async void RestartGame()
        {
            changePhaseFadePanel.HideFade();
            endGamePanel.Hide();
            await introManager.RestartGame();
            changePhaseFadePanel.ShowFade();


            eventMode.RestartGameMode();
            restMode.RestartGameMode();
            OnEventModeEnded(GameState.ONGOING);
        }

        void OnDestroy()
        {
            if (eventMode)
            {
                eventMode.OnGameModeEnded -= OnEventModeEnded;
                eventMode.OnGameModeInterrupted -= SaveGameModeDatas;
            }

            if (restMode)
            {
                restMode.OnGameModeEnded -= OnRestModeEnded;
                restMode.OnGameModeInterrupted -= SaveGameModeDatas;
            }

            if (menuPanel)
            {
                menuPanel.OnNewGameAsked -= LaunchGame;
                menuPanel.OnContinueGameAsked -= ContinueGame;
                menuPanel.OnQuitGameAsked -= Application.Quit;
            }

            if (endGamePanel)
            {
                endGamePanel.OnRestartCalled -= RestartGame;
            }
        }
    }
}