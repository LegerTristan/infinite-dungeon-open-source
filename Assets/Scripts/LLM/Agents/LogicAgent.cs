using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LingeringHopes.LargeLanguageModel
{
    public class LogicAgent : CustomAgent
    {
        [SerializeField]
        string actionModularBaseGBNF = string.Empty;

        [SerializeField]
        string actionModularSeparatorGBNF = string.Empty;

        [SerializeField, TextArea(3, 10)]
        string actionGBNF = string.Empty;

        public async override Task<string> Chat(PromptParameters promptParameters, Action<string, bool, bool> streamCallback = null)
        {
            string response = string.Empty;

            if (!IsWarmedUp())
            {
                return response;
            }

            try
            {
                if (promptParameters is LogicPromptParameters logicParameters)
                {
                    isProcessing = true;
                    await agent.ClearHistory();

                    agent.grammar = FormatActionGBNF(logicParameters.NbrActions);

                    string prompt = FormatActionPrompt(logicParameters);
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
                Debug.LogError($"[LogicAgent] => Error during text generation : {ex.Message}");
                streamCallback?.Invoke(response, true, true); 
                return null;
            }
            finally
            {
                isProcessing = false;
            }

            return response;
        }

        string FormatActionGBNF(int nbrActions)
        {
            StringBuilder finalGBNF = new StringBuilder();
            finalGBNF.Append("root ::= ");
            finalGBNF.Append(actionModularBaseGBNF);

            for (int i = 1; i < nbrActions; i++)
            {
                finalGBNF.Append(' ');
                finalGBNF.Append(actionModularSeparatorGBNF);
                finalGBNF.Append(' ');
                finalGBNF.Append(actionModularBaseGBNF);
            }

            finalGBNF.Append('\n');
            finalGBNF.Append(actionGBNF);
            return finalGBNF.ToString();
        }

        string FormatActionPrompt(LogicPromptParameters logicPromptParams)
        {
            return $"Suggest {logicPromptParams.NbrActions} actions to resolve the event.\n" +
                   $"Event : {logicPromptParams.EventNarration}\n" +
                   $"Required action to resolve the event : {logicPromptParams.EventHint}";
        }
    }
}