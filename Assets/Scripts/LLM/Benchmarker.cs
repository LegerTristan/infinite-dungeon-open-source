using LLMUnity;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LingeringHopes.Gameplay;
using System.Diagnostics;

namespace LingeringHopes.LargeLanguageModel
{
    public abstract class Benchmarker : BaseComponent
    {
        [Header("Managers")]
        [SerializeField]
        protected LLM llm = null;

        [SerializeField]
        protected LLMManager llmManager = null;

        [SerializeField]
        string csvName = "benchmark_results";

        [Header("Prompts Configuration")]
        [SerializeField, TextArea(3, 10)]
        protected string systemPrompt = null;

        protected readonly List<BenchmarkResult> currentSessionResults = new List<BenchmarkResult>();

        protected readonly Stopwatch timer = new Stopwatch();

        protected float ttftTime = 0f;

        protected int tokenCount = 0;

        protected bool firstTokenReceived = false;

        protected override bool IsValid() => llm && llmManager && !string.IsNullOrEmpty(systemPrompt) && !string.IsNullOrEmpty(csvName);

        void OnEnable()
        {
            if(llmManager != null)
            {
                llmManager.OnAIStreaming += OnTokenReceived;
            }
        }

        protected void LogSetupProgress(float progress)
        {
            //UnityEngine.Debug.Log("Setup progress : " + progress);
        }

        protected void PrepareForNewRequest()
        {
            firstTokenReceived = false;
            tokenCount = 0;
            ttftTime = 0;
        }

        public void OnTokenReceived(string chunk)
        {
            if (!firstTokenReceived)
            {
                ttftTime = (float)timer.Elapsed.TotalSeconds;
                firstTokenReceived = true;
            }
            tokenCount++;
        }

        protected void RecordStep(string technique, string input, string response, float totalDuration)
        {
            if(string.IsNullOrEmpty(response) || string.IsNullOrEmpty(technique) || string.IsNullOrEmpty(input))
            {
                UnityEngine.Debug.LogWarning("[Benchmarker] => Step not recorded. A parameter is missing !");
                return;
            }

            float streamingDuration = totalDuration - ttftTime;
            float tps = (firstTokenReceived && streamingDuration > 0.001f)
                ? tokenCount / streamingDuration
                : 0f;

            currentSessionResults.Add(new BenchmarkResult
            {
                ModelName = llm != null ? llm.model : "Unknown",
                PromptStep = technique,
                UserInput = input,
                FullResponse = response.Replace("\n", " ").Replace(";", ","),
                TTFT = firstTokenReceived ? ttftTime : 0f,
                TPS = tps,
                TotalDuration = totalDuration
            });

            UnityEngine.Debug.Log($"[Benchmarker] => Step recorded ! Number results {currentSessionResults.Count}");
        }

        public void ExportToCSV()
        {
            string path = Path.Combine(Application.persistentDataPath, $"{csvName}.csv");
            bool fileExists = File.Exists(path);

            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                if (!fileExists)
                {
                    sw.WriteLine("Model;Step;Input;TTFT;TPS;Duration;Response");
                }

                foreach (BenchmarkResult result in currentSessionResults)
                {
                    string cleanInput = CleanCsvField(result.UserInput);
                    string cleanResponse = CleanCsvField(result.FullResponse);

                    sw.WriteLine($"{result.ModelName};{result.PromptStep};\"{cleanInput}\";{result.TTFT:F2};{result.TPS:F2};{result.TotalDuration:F2};\"{cleanResponse}\"");
                }
            }
            currentSessionResults.Clear();
        }

        string CleanCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            return field.Replace("\n", " ").Replace("\r", " ").Replace(";", ",");
        }

        void OnDisable()
        {
            if (llmManager != null)
            {
                llmManager.OnAIStreaming -= OnTokenReceived;
            }
        }
    }

}