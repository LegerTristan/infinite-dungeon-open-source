using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LingeringHopes.LargeLanguageModel
{
    public class ItemAgent : CustomAgent
    {
        public async override Task<string> Chat(PromptParameters promptParameters, Action<string, bool, bool> streamCallback = null)
        {
            string response = string.Empty;

            if (!IsWarmedUp())
            {
                return response;
            }

            try
            {
                if (promptParameters is ItemPromptParameters itemParameters)
                {
                    isProcessing = true;
                    await agent.ClearHistory();

                    string prompt = $"Item name: {itemParameters.ItemName}";
                    //await GetCurrentSessionTokenUsage(prompt);

                    CallOnAgentProcessing($"{agent.systemPrompt}\n{prompt}");
                    response = await agent.Chat(prompt, (chunk) =>
                    {
                        streamCallback?.Invoke(chunk, false, false);
                    }, null, false);

                    streamCallback?.Invoke(response, true, false);
                    return response;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemAgent] => Error during text generation: {ex.Message}");
                streamCallback?.Invoke(response, true, true); 
                return null;
            }
            finally
            {
                isProcessing = false;
            }

            return response;
        }
    }
}