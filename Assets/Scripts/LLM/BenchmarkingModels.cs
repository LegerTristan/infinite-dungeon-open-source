using System;

namespace LingeringHopes.LargeLanguageModel
{
    public enum PromptType
    {
        Narration,
        Logic
    }

    [Serializable]
    public class BenchmarkResult
    {
        public string ModelName;
        public string PromptStep;
        public string UserInput;
        public string FullResponse;
        public float TTFT; // Time To First Token (seconds)
        public float TPS;  // Tokens Per Second
        public float TotalDuration;
    }
}
