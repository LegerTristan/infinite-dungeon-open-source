using System.Threading.Tasks;
using UnityEngine;
using LingeringHopes.Gameplay;
using LingeringHopes.LargeLanguageModel;
using LingeringHopes.UI;

namespace LingeringHopes.Flow
{
    public class IntroManager : BaseComponent
    {
        #region Fields
        [SerializeField]
        LLMManager diagenManager = null;

        [SerializeField]
        IntroductionPanel introPanel = null;

        [SerializeField]
        WarmupPanel warmupPanel = null;
        #endregion

        protected override bool IsValid() => diagenManager && introPanel && warmupPanel;

        #region Setup

        protected override void Start()
        {
            base.Start();
            introPanel.gameObject.SetActive(false);
            warmupPanel.Close();
        }

        public async Task InitializeGame()
        {
            Task warmupTask = diagenManager.FullWarmup(
                onProgress: (stepDescription, progress) => warmupPanel.SetStep(stepDescription, progress)
            );

            introPanel.gameObject.SetActive(true);
            await introPanel.DisplayIntroduction();
            introPanel.gameObject.SetActive(false);

            if (!warmupTask.IsCompleted)
            {
                warmupPanel.Display();
                await warmupTask;
                warmupPanel.Close();
            }
        }

        public async Task RestartGame()
        {
            introPanel.gameObject.SetActive(true);
            Task introTask = introPanel.DisplayIntroduction();
            Task restartTask = diagenManager.Restart();

            await Task.WhenAll(introTask, restartTask);

            introPanel.gameObject.SetActive(false);
        }
        #endregion
    }

}