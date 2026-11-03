using System;

namespace LingeringHopes.LargeLanguageModel
{
    public interface ILLMProvider
    {
        event Action<string, string> OnRequestStarted; // Prompt, Technique
        event Action<string> OnAIStreaming;           // Chunks
        event Action<string> OnAIResponded;           // Final Response

        string GetModelName();
    }
}
