using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class DownloadPanel : BaseComponent
    {
        [Header("Panel Containers")]
        [SerializeField]
        GameObject errorText = null;

        [SerializeField]
        GameObject progressPanel = null;

        [SerializeField]
        GameObject downloadPanel = null;

        [Header("Progress Display")]
        [SerializeField]
        Scrollbar progressBar = null;

        [SerializeField]
        TMP_Text progressText = null;

        protected override bool IsValid() => downloadPanel && progressPanel && progressBar && progressText && errorText;

        public void Display()
        {
            errorText.SetActive(false);
            progressPanel.SetActive(true);
        }

        public void SetProgress(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            progressText.text = $"{Mathf.RoundToInt(clampedProgress * 100f)}%";
            progressBar.size = clampedProgress;
        }

        public void Close()
        {
            progressPanel.SetActive(false);
        }

        public void DisplayError()
        {
            progressPanel.SetActive(false);
            errorText.SetActive(true);
        }

    }

}