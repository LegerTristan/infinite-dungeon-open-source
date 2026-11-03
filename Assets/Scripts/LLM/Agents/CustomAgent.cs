using LLMUnity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LingeringHopes.LargeLanguageModel
{
    [RequireComponent(typeof(LLMAgent))]
    public abstract class CustomAgent : MonoBehaviour, ILLMAgent
    {
        public event Action<string> OnAgentProcessing = null;

        protected LLMAgent agent = null;

        protected bool isProcessing = false;

        bool isWarmingUp = false, warmupDone = false;

        public bool IsWarmedUp() => isWarmingUp || warmupDone;

        public bool IsProcessing() => isProcessing;

        protected void CallOnAgentProcessing(string fullPrompt) => OnAgentProcessing?.Invoke(fullPrompt);

        void Awake()
        {
            agent = GetComponent<LLMAgent>();
        }

        public async Task Warmup(Action callback = null)
        {
            isWarmingUp = true;
            await agent.Warmup(callback);

            isWarmingUp = false;
            warmupDone = true;
        }

        public void UpdateHistory(List<ChatMessage> newHistory)
        {
            if (agent)
            {
                agent.chat = newHistory;
            }

        }

        public abstract Task<string> Chat(PromptParameters promptParameters, Action<string, bool, bool> streamCallback = null);

        protected async Task<int> GetCurrentSessionTokenUsage(string userPrompt)
        {
            if (!agent || agent.chat == null)
            {
                return 0;
            }

            StringBuilder fullSessionText = new StringBuilder();
            fullSessionText.Append(agent.systemPrompt);

            for (int i = 0; i < agent.chat.Count; i++)
            {
                fullSessionText.Append(agent.chat[i].role);
                fullSessionText.Append(agent.chat[i].content);
            }

            fullSessionText.Append(userPrompt);
            List<int> tokens = await agent.Tokenize(fullSessionText.ToString());

            int templatePadding = (agent.chat.Count + 1) * 4;
            int totalTokens = tokens.Count + templatePadding;

            return totalTokens;
        }

        public void CancelRequest()
        {
            if (isProcessing && agent != null)
            {
                agent.CancelRequests();
            }
        }

        public async Task Reset()
        {
            if (agent != null)
            {
                await agent.ClearHistory();

            }
        }
    }
}