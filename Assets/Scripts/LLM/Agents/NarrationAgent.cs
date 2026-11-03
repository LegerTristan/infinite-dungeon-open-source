using LLMUnity;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LingeringHopes.Gameplay;

namespace LingeringHopes.LargeLanguageModel
{
    [RequireComponent(typeof(LLMAgent))]
    public class NarrationAgent : CustomAgent
    {
        [Header("Prompt Templates")]
        [SerializeField]
        SatietyPromptPair[] satietyPrompts = null;

        [SerializeField, TextArea(3, 5)]
        string startNarrationPrompt = null;

        [SerializeField, TextArea(3, 5)]
        string continueNarrationPrompt = null;

        [SerializeField, TextArea(3, 5)]
        string narrationOutcomePrompt = null;

        [SerializeField, TextArea(3, 5)]
        string runOutOfTimePrompt = null;

        [SerializeField, TextArea(3, 5)]
        string desperateActionPrompt = null;

        public async override Task<string> Chat(PromptParameters promptParameters, Action<string, bool, bool> streamCallback = null)
        {
            string response = string.Empty;
            if (!IsWarmedUp())
            {
                return response;
            }

            try
            {
                isProcessing = true;
                switch (promptParameters)
                {
                    case NarrationStartPromptParameters startParams:
                        response = await NarrateBeginning(startParams, streamCallback);
                        break;
                    case NarrationContinuePromptParameters continueParams:
                        response = await NarrateOngoing(continueParams, streamCallback);
                        break;
                    case NarrationEndingPromptParameters endParams:
                        response = await NarrateEnding(endParams, streamCallback);
                        break;
                    default:
                        Debug.LogWarning($"[NarrationAgent] Prompt type not supported : {promptParameters?.GetType().Name}");
                        response = string.Empty;
                        break;
                }

                streamCallback?.Invoke(response, true, false);
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NarrationAgent] => Error during text generation : {ex.Message}");
                streamCallback?.Invoke(response, true, true);
                return null;
            }
            finally
            {
                isProcessing = false;
            }
        }

        #region StartNarration
        async Task<string> NarrateBeginning(NarrationStartPromptParameters parameters, Action<string, bool, bool> streamCallback = null)
        {
            string prompt = GenerateNarrationStartPrompt(parameters);
            //await GetCurrentSessionTokenUsage(prompt);

            CallOnAgentProcessing($"{agent.systemPrompt}\n{prompt}");
            string response = await agent.Chat(prompt, (chunk) =>
            {
                streamCallback?.Invoke(chunk, false, false);
            }, null, false);

            return response;
        }

        public string GenerateNarrationStartPrompt(NarrationStartPromptParameters parameters)
        {
            string currentSatietyPrompt = GetSatietyPrompt(parameters.Satiety);

            return $"{startNarrationPrompt}\n " +
                   $"{FormatEnvironmentTag(parameters.EnvironmentTag)}\n" +
                   $"{FormatEventTag(parameters.EventTag)}\n " +
                   $"{currentSatietyPrompt}\n " +
                   $"{(parameters.PlayerStarving ? desperateActionPrompt : string.Empty)}".TrimEnd();
        }

        string GetSatietyPrompt(float satiety)
        {
            if (satietyPrompts == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < satietyPrompts.Length; i++)
            {
                if (satiety >= satietyPrompts[i].MinSatietyValue)
                {
                    return satietyPrompts[i].Prompt;
                }
            }

            // By default, return an empty string if no prompt matches the current satiety value. 
            return string.Empty;
        }

        static string FormatEnvironmentTag(EnvironmentTag environmentTag)
        {
            return environmentTag != null ? $"Environment: {environmentTag} {environmentTag.GetRandomDecorationInformation()}" : string.Empty;
        }


        static string FormatEventTag(EventTag eventTag)
        {
            return eventTag != null ? $"Event: {eventTag}\n" : string.Empty;
        }
        #endregion

        #region ContinueNarration
        async Task<string> NarrateOngoing(NarrationContinuePromptParameters parameters, Action<string, bool, bool> streamCallback = null)
        {
            string prompt = GenerateNarrationPrompt(parameters);
            //await GetCurrentSessionTokenUsage(prompt);

            CallOnAgentProcessing($"{agent.systemPrompt}\n{prompt}");
            return await agent.Chat(prompt, (chunk) =>
            {
                streamCallback?.Invoke(chunk, false, false);
            }, null, false);
        }

        public string GenerateNarrationPrompt(NarrationContinuePromptParameters parameters)
        {
            return $"{continueNarrationPrompt}\n" +
                $"{ParseUserActionResponse(parameters.UserAction)}\n" +
                $"{(parameters.IsRunningOutOfTime ? runOutOfTimePrompt : "")}";
        }

        string ParseUserActionResponse(ActionResponse action, bool detailed = false)
        {
            if (action == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Player decides to {action.action}");

            if (detailed)
            {
                string progressImpact = ParseActionParameterValue(
                    action.progression,
                    "The player should progress towards the exit.",
                    "The player should be slowed down or blocked"
                );
                string satietyImpact = ParseActionParameterValue(
                    action.satiety,
                    "The player should feel sated or less hungry",
                    "The player should feel hungrier or exhausted"
                );
                string itemImpact = !string.Equals(action.itemCollected, "None", StringComparison.OrdinalIgnoreCase)
                    ? $"Player has collected the item \"{action.itemCollected}\""
                    : string.Empty;

                builder.AppendLine(progressImpact);
                builder.AppendLine(satietyImpact);
                builder.AppendLine(itemImpact);
            }

            return builder.ToString().Trim();
        }

        string ParseActionParameterValue(string value, string increasedText, string decreasedText)
        {
            return value switch
            {
                "INCREASED" => increasedText,
                "DECREASED" => decreasedText,
                _ => string.Empty
            };
        }
        #endregion

        #region EndingNarration
        async Task<string> NarrateEnding(NarrationEndingPromptParameters parameters, Action<string, bool, bool> streamCallback = null)
        {
            string prompt = GenerateNarrationOutcomePrompt(parameters);
            //await GetCurrentSessionTokenUsage(prompt);

            CallOnAgentProcessing($"{agent.systemPrompt}\n{prompt}");
            return await agent.Chat(prompt, (chunk) =>
            {
                streamCallback?.Invoke(chunk, false, false); 
            }, null, false);
        }

        string GenerateNarrationOutcomePrompt(NarrationEndingPromptParameters parameters)
        {
            string outOfTimeText = parameters.RunnedOutOfTime ? "Player ran out of time and could not resolve the event.\n" : string.Empty;
            return $"{outOfTimeText}{narrationOutcomePrompt}\n\"{ParseUserActionResponse(parameters.UserAction, true)}\"";
        }
        #endregion
    }
}
