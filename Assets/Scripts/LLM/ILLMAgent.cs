using LLMUnity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LingeringHopes.LargeLanguageModel
{
    public interface ILLMAgent
    {
        event Action<string> OnAgentProcessing;

        bool IsWarmedUp();

        bool IsProcessing();

        Task Warmup(Action callback = null);

        void UpdateHistory(List<ChatMessage> newHistory);

        Task<string> Chat(PromptParameters promptParameters, Action<string, bool, bool> streamCallback = null);

        void CancelRequest();

        Task Reset();
    }
}

