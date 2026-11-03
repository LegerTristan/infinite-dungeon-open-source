using LingeringHopes.Gameplay;
using LingeringHopes.LargeLanguageModel;
using LingeringHopes.UI;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LingeringHopes.Flow
{
    public class DownloadManager : BaseComponent
    {
        #region Fields
        [SerializeField]
        LLMManager diagenManager = null;

        [SerializeField]
        DownloadPanel downloadPanel = null;
        #endregion

        protected override bool IsValid() => diagenManager && downloadPanel;

        void OnEnable()
        {
            if (downloadPanel)
            {
                downloadPanel.gameObject.SetActive(true);
            }
        }

        void OnDisable()
        {
            if (downloadPanel)
            {
                downloadPanel.gameObject.SetActive(false);
            }
        }

        #region Setup
        public async Task<bool> DownloadAssets()
        {
            Debug.Log("Download Assets...");

            downloadPanel.Display();
            bool isInit = await diagenManager.InitLLMServer(downloadPanel.SetProgress);
            if (!isInit)
            {
                Debug.LogWarning("InitLLMServer failed !!");
                downloadPanel.DisplayError();
                return false;
            }
            downloadPanel.Close();
            return true;
        }       
        #endregion
    }

}