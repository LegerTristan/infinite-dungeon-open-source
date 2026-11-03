using LingeringHopes.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LingeringHopes.UI
{
    public class WarmupPanel : BaseComponent
    {
        [Header("Containers")]
        [SerializeField] 
        GameObject contentPanel = null;

        [Header("Progress Display")]
        [SerializeField] 
        Scrollbar progressBar = null;
        
        [SerializeField] 
        TMP_Text progressText = null;
        
        [SerializeField] 
        TMP_Text statusText = null;

        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 5f)] 
        float barAnimationSpeed = 1.5f;
        
        [SerializeField, Range(0.1f, 1f)] 
        float dotsAnimationInterval = 0.4f;

        float targetProgress = 0f;
        float currentProgress = 0f;
        
        float dotsTimer = 0f;
        int dotCount = 0;
        string currentBaseStatus = "Warming up AI";

        protected override bool IsValid() => contentPanel && progressBar && progressText && statusText;

        void Update()
        {
            if (!contentPanel.activeSelf)
            {
                return;
            }

            if (!Mathf.Approximately(currentProgress, targetProgress))
            {
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, barAnimationSpeed * Time.deltaTime);
                progressBar.size = currentProgress;
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100f)}%";
            }

            dotsTimer += Time.deltaTime;
            if (dotsTimer >= dotsAnimationInterval)
            {
                dotsTimer = 0f;
                dotCount = (dotCount + 1) % 4;
                statusText.text = currentBaseStatus + new string('.', dotCount);
            }
        }

        public void Display()
        {
            currentProgress = 0f;
            targetProgress = 0f;
            progressBar.size = 0f;
            progressText.text = "0%";
            currentBaseStatus = "Starting local AI engine";

            contentPanel.SetActive(true);
        }

        public void SetStep(string stepDescription, float progress)
        {
            currentBaseStatus = stepDescription;
            targetProgress = Mathf.Clamp01(progress);
            progressText.text = $"{progress * 100}%";
        }

        public void Close()
        {
            contentPanel.SetActive(false);
        }
    }
}