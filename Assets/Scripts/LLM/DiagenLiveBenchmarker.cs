using UnityEngine;

namespace LingeringHopes.LargeLanguageModel
{
    public class DiagenLiveBenchmarker : Benchmarker
    {
        string currentTechnique = string.Empty;

        void Awake()
        {
            if (llmManager != null)
            {
                llmManager.OnRequestStarted += StartTracking; // Ensure LLMLiveBenchmarker is the first suscribed to the LLMManager
                llmManager.OnAIResponded += StopTrackingAndSave;
            }
        }

        void StartTracking(string prompt, string technique)
        {
            Debug.Log($"[DiagenLiveBenchmarker] => Prompt \"{prompt}\", technique \"{technique}\"");

            PrepareForNewRequest();
            systemPrompt = prompt;
            currentTechnique = technique;

            timer.Restart();
        }

        void StopTrackingAndSave(string finalResponse)
        {
            timer.Stop();

            RecordStep(currentTechnique, systemPrompt, finalResponse, (float)timer.Elapsed.TotalSeconds);
            ExportToCSV();
        }

        void OnDestroy()
        {
            if (llmManager != null)
            {
                llmManager.OnRequestStarted -= StartTracking;
                llmManager.OnAIResponded -= StopTrackingAndSave;
            }
        }
    }
}