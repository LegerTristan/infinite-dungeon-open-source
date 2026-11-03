using LingeringHopes.LargeLanguageModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using LingeringHopes.Gameplay;
using LingeringHopes.UI;

using Random = UnityEngine.Random;

namespace LingeringHopes.Flow
{
    enum EventState
    {
        Starting,
        Narrating,
        AwaitingAction,
        Ending,
        CreatingItem,
        Completed
    }

    public class EventMode : GameMode
    {
        #region Fields
        [Header("Event Managers")]
        [SerializeField]
        LLMManager llmManager = null;

        [SerializeField]
        ItemFactory itemFactory = null;

        [SerializeField]
        NumberActionsManager numberActionsManager = null;

        [Header("Configuration")]
        [SerializeField]
        TagLibrary tagLibrary = null;

        [SerializeField]
        EventPanel eventPanel = null;

        readonly TagRepetabilityHandler repeatabilityHandler = new TagRepetabilityHandler();

        EventState currentEventState = EventState.Narrating;

        [SerializeField, Range(1, 10)]
        int maxNarrationCount = 3;

        [Header("Timeout Configuration")]
        [SerializeField]
        float narrationTimeoutDuration = 15f;

        #region Runtime State
        List<ActionResponse> currentActionResponses = new List<ActionResponse>();

        ActionResponse lastPlayerAction = null;

        EventTag evtTag = null;

        EnvironmentTag envTag = null;

        CancellationTokenSource narrationTimeoutCts = null;

        string lastNarration = null;

        string lastItemCollected = null;

        string currentEventHint = null;

        float satietyScore = 0f, progressionScore = 0f, itemCollectedScore = 0f, nbrLogicStep = 0f;

        int nbrActions = 0, latestActionIndex = 0;

        int tokenLength = 0;

        int narrationCount = 0;

        bool isInDesperateAction = false;
        #endregion
        #endregion

        #region Regex
        static readonly Regex JsonExtractorRegex = new Regex(@"\{.*\}", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex MarkdownBlockRegex = new Regex(@"```json|```", RegexOptions.Compiled);
        static readonly Regex JsonStructureRegex = new Regex(@"\""\w+\""\s*:\s*|[\{\}\[\]]", RegexOptions.Compiled);
        static readonly Regex MultiSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        #endregion

        protected override bool IsValid() => base.IsValid() && itemFactory && tagLibrary && eventPanel && llmManager && numberActionsManager;

        protected override void Start()
        {
            base.Start();

            llmManager.OnAIStreaming += HandleAIStreaming;
            llmManager.OnAIResponded += HandleAIResponse;
            llmManager.OnAIProcessFailed += HandleAIError;

            eventPanel.OnUserActionSelected += SendUserActionToAI;
            eventPanel.OnConfirmClicked += EndEventMode;
            eventPanel.OnRelaunchLLMProcessClicked += RelaunchLLMRequest;

            eventPanel.Initialize();
        }

        public override void RestoreGameModeDatas(SavedDatas datas)
        {
            base.RestoreGameModeDatas(datas);
            llmManager.FullWarmup(onComplete: PrepareEvent);
        }

        public void PrepareEvent()
        {
            currentEventState = EventState.Starting;
            narrationCount = 0;
            isInDesperateAction = satietyManager.IsStarving;

            satietyScore = 0;
            progressionScore = 0;
            itemCollectedScore = 0;
            nbrLogicStep = 0;
            latestActionIndex = 0;
            tokenLength = 0;
            currentActionResponses.Clear();

            eventPanel.ToggleConfirmButton(false);
            eventPanel.HideActions();

            repeatabilityHandler.UpdateUsedTags();

            envTag = tagLibrary.GetRandomTagOfType<EnvironmentTag>(TagType.ENVIRONMENT, repeatabilityHandler.IsTagAvailable);
            evtTag = envTag.GetRandomCompatibleEvent(repeatabilityHandler.IsTagAvailable);

            currentEventHint = evtTag.Hint;
            maxNarrationCount = evtTag.MaxDuration;
            nbrActions = numberActionsManager.GetNumberActions(exitManager.CurrentProgress);
            eventPanel.CreateActions(nbrActions);

            // Add used tag to the repeatability handler to refresh them after a number of turn.
            repeatabilityHandler.AddUsedTag(envTag);
            repeatabilityHandler.AddUsedTag(evtTag);

            eventPanel.AddPlaceholderTextToHistory();
            StartNarrationTimer();

            llmManager.NarrateStartEvent(envTag, evtTag, currentEventHint, satietyManager.CurrentSatiety, isInDesperateAction);
        }

        void HandleAIStreaming(string token)
        {
            if (currentEventState == EventState.Starting || currentEventState == EventState.Narrating || currentEventState == EventState.Ending)
            {
                StopNarrationTimer();
                eventPanel.DisplayStreamingToken(token);
            }
            else if (currentEventState == EventState.AwaitingAction)
            {
                string addedPart = token.Substring(tokenLength);

                if (addedPart.Contains('{'))
                {
                    AddAction();
                }
                else if (addedPart.Contains('}'))
                {
                    string[] jsonResponses = token.Split(";");

                    // We already have all the available actions. We stop here.
                    if (currentActionResponses.Count == jsonResponses.Length)
                    {
                        return;
                    }

                    string jsonResponse = jsonResponses[jsonResponses.Length - 1];
                    AddActionText(jsonResponse);
                }

                tokenLength = token.Length;
            }
        }

        void HandleAIResponse(string response)
        {
            StopNarrationTimer();

            switch (currentEventState)
            {
                case EventState.Starting:
                    StartNarration(response);
                    break;

                case EventState.AwaitingAction:
                    ProcessAction(response);
                    break;

                case EventState.Narrating:
                    ProcessNarrationStep(response);
                    break;

                case EventState.Ending:
                    EndNarration(response);
                    break;

                case EventState.CreatingItem:
                    AddGenericItemToInventory(response);
                    break;
            }
        }

        void HandleAIError()
        {
            StopNarrationTimer();
            eventPanel.AddErrorTextToHistory();
            eventPanel.ToggleRelaunchButton(true);
        }

        #region Timeout Handling
        void StartNarrationTimer()
        {
            StopNarrationTimer();
            eventPanel.ToggleRelaunchButton(false);

            narrationTimeoutCts = new CancellationTokenSource();
            _ = WatchNarrationTimeoutAsync(narrationTimeoutCts.Token);
        }

        void StopNarrationTimer()
        {
            if (narrationTimeoutCts != null)
            {
                narrationTimeoutCts.Cancel();
                narrationTimeoutCts.Dispose();
                narrationTimeoutCts = null;
            }
        }

        async Task WatchNarrationTimeoutAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(narrationTimeoutDuration * 1000), token);

                if (!token.IsCancellationRequested)
                {
                    if (currentEventState == EventState.Starting || currentEventState == EventState.Narrating || currentEventState == EventState.Ending)
                    {
                        Debug.LogWarning($"[EventMode] => Narration generation timeout ({narrationTimeoutDuration}s). Displaying relaunch button.");
                        eventPanel.ToggleRelaunchButton(true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[EventMode] => Narration done and timer successfully stopped.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventMode] => Unexpected error in timeout watcher: {ex.Message}");
            }
        }
        #endregion

        #region EventPhases
        void StartNarration(string jsonResponse)
        {
            NarrationResponse narrationResponse = SerializeAIResponse<NarrationResponse>(jsonResponse);
            lastNarration = narrationResponse?.narration ?? CleanResponseFromJSON(jsonResponse);
            eventPanel.AddTextToHistory(lastNarration);
            AddLatestMessageToCondensedHistory("user", $"Environment: {envTag.name}, Event: {evtTag.name}");
            AddLatestMessageToCondensedHistory("assistant", $"{lastNarration}");

            eventPanel.AddPlaceholderTextToHistory("Creating actions...");
            currentEventState = EventState.AwaitingAction;
            llmManager.CreateAction(StripQuestionsFromNarration(lastNarration), currentEventHint, nbrActions);
        }

        string StripQuestionsFromNarration(string narration)
        {
            if (string.IsNullOrEmpty(narration))
            {
                return narration;
            }

            int questionMarkIndex = narration.IndexOf("?");
            if (questionMarkIndex == -1)
            {
                return narration;
            }

            int dotIndex = narration.LastIndexOf(".");
            if (dotIndex != -1)
            {
                return narration.Substring(0, dotIndex + 1);
            }
            else
            {
                return narration.Substring(0, questionMarkIndex);
            }
        }

        void ProcessAction(string aiResponse)
        {
            // Reset action streaming values for next process.
            latestActionIndex = 0;
            tokenLength = 0;

            currentEventState = EventState.Narrating;
            eventPanel.SetActionButtonState(true);
        }

        void AddAction()
        {
            latestActionIndex = eventPanel.AddAction();
        }

        void AddActionText(string jsonResponse)
        {
            ActionResponse actionResponse = SerializeAIResponse<ActionResponse>(jsonResponse.Trim());
            if (actionResponse == null)
            {
                Debug.LogWarning("[EventMode] => actionResponse could not be deserialized.");
                actionResponse = new ActionResponse();
                actionResponse.action = CleanResponseFromJSON(jsonResponse);
            }
            else
            {
                actionResponse.action = CleanResponse(actionResponse.action);
            }

            eventPanel.AddTextToAction(actionResponse.action, latestActionIndex - 1);
            currentActionResponses.Add(actionResponse);
        }

        void ProcessNarrationStep(string jsonResponse)
        {
            NarrationResponse narrationResponse = SerializeAIResponse<NarrationResponse>(jsonResponse);
            lastNarration = narrationResponse?.narration ?? CleanResponseFromJSON(jsonResponse);
            eventPanel.AddTextToHistory(lastNarration);
            AddLatestMessageToCondensedHistory("assistant", $"{lastNarration}");

            narrationCount++;
            currentEventState = EventState.AwaitingAction;

            StartCoroutine(NextFrameAction(() => llmManager.CreateAction(StripQuestionsFromNarration(lastNarration), currentEventHint, nbrActions)));
        }

        void EndNarration(string jsonResponse)
        {
            NarrationResponse narrationResponse = SerializeAIResponse<NarrationResponse>(jsonResponse);
            string narration = narrationResponse?.narration ?? CleanResponseFromJSON(jsonResponse);
            eventPanel.AddTextToHistory(narration);

            AddLatestMessageToCondensedHistory("assistant", $"{narration}"); // Done for consistency purpose but not useful at this step of the event.

            currentEventState = EventState.Completed;
            InterpretConsequences();
        }

        // Prevents race conditions during benchmark response tracking.
        IEnumerator NextFrameAction(Action callback)
        {
            yield return new WaitForEndOfFrame();

            callback?.Invoke();
        }
        #endregion

        #region Consequences
        void InterpretConsequences()
        {
            if (nbrLogicStep > 0f)
            {
                satietyScore = satietyScore / nbrLogicStep;
                progressionScore = progressionScore / nbrLogicStep;
                itemCollectedScore = itemCollectedScore / nbrLogicStep;
            }

            InterpretConsequenceScore(ApplySatietyConsequence, satietyScore);
            InterpretConsequenceScore(ApplyProgressionConsequence, progressionScore);

            if (itemCollectedScore > 0.1f)
            {
                eventPanel.ShowItemConsequence($"You found a(n) {lastItemCollected}!", Color.green);
                CreateGenericItem();
            }
            else
            {
                eventPanel.ToggleConfirmButton(true);
            }
        }

        void InterpretConsequenceScore(Action<bool> consequenceCallback, float score)
        {
            if (score > 0.2f)
            {
                consequenceCallback?.Invoke(true);
            }
            else if (score < -0.2f)
            {
                consequenceCallback?.Invoke(false);
            }
        }

        void ApplySatietyConsequence(bool result)
        {
            if (result)
            {
                satietyManager.IncreaseSatiety(Random.Range(0.05f, 0.15f));
                eventPanel.ShowSatietyConsequence($"Satiety increased !", Color.green);
            }
            else
            {
                satietyManager.ReduceSatiety(Random.Range(0.05f, 0.15f));
                eventPanel.ShowSatietyConsequence($"Satiety decreased !", Color.red);
            }
        }

        void ApplyProgressionConsequence(bool result)
        {
            if (result)
            {
                exitManager.ProgressThroughExit(Random.Range(0.05f, 0.15f));
                eventPanel.ShowProgressionConsequence($"Progression increased !", Color.green);
            }
            else
            {
                exitManager.Regress(Random.Range(0.05f, 0.15f));
                eventPanel.ShowProgressionConsequence($"Progression decreased !", Color.red);
            }
        }

        void InterpretStringChange(string value, Action decreaseCallback, Action increaseCallback, Action<string, Color> uiCallback,
            string variableName)
        {
            switch (value)
            {
                case "INCREASED":
                    increaseCallback?.Invoke();
                    uiCallback?.Invoke($"{variableName} increased !", Color.green);
                    break;
                case "DECREASED":
                    decreaseCallback?.Invoke();
                    uiCallback?.Invoke($"{variableName} decreased !", Color.red);
                    break;
            }
        }

        void ComputeIntermediateConsequences(LogicResponse responseData, float weight = 1)
        {
            InterpretStringChange(responseData.satiety, () => satietyScore -= weight, () => satietyScore += weight, null, "");
            InterpretStringChange(responseData.progression, () => progressionScore -= weight, () => progressionScore += weight, null, "");

            if (responseData.itemCollected.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                itemCollectedScore -= weight;
            }
            else
            {
                itemCollectedScore += weight;
                lastItemCollected = responseData.itemCollected;
            }

            nbrLogicStep += weight;
        }
        #endregion

        #region Items
        void CreateGenericItem()
        {
            currentEventState = EventState.CreatingItem;
            eventPanel.AddPlaceholderTextToHistory("Creating new item...");
            StartCoroutine(NextFrameAction(() => llmManager.CreateItem(lastItemCollected)));
        }

        void AddGenericItemToInventory(string response)
        {
            ItemResponse itemResponse = SerializeAIResponse<ItemResponse>(response);
            if (itemResponse != null)
            {
                FoodItem newItem = itemFactory.CreateDynamicFood(
                    lastItemCollected,
                    itemResponse.itemDescription,
                    itemResponse.satietyGain
                );
                inventoryManager.AddItem(newItem);
            }
            else
            {
                Debug.LogWarning("[EventMode] => Generic item not valid ! Canceling item creation...");
            }

            eventPanel.ToggleConfirmButton(true);
        }
        #endregion

        void SendUserActionToAI(int userActionIndex)
        {
            ActionResponse userActionResponse = currentActionResponses[userActionIndex];
            lastPlayerAction = userActionResponse;

            eventPanel.AddTextToHistory($"<color=yellow>{userActionResponse.action}</color>");
            eventPanel.HideActions();
            eventPanel.SetActionButtonState(false);

            currentActionResponses.Clear();

            ProcessUserAction(userActionResponse);
            ComputeIntermediateConsequences(userActionResponse);
        }

        void ProcessUserAction(ActionResponse userAction, bool addActionToHistory = true)
        {
            eventPanel.AddPlaceholderTextToHistory();
            bool runnedOutOfTime = narrationCount >= maxNarrationCount;

            if (runnedOutOfTime || userAction.ended == "YES")
            {
                currentEventState = EventState.Ending;
                llmManager.NarrateEndEvent(userAction, runnedOutOfTime);
            }
            else
            {
                if (addActionToHistory)
                    AddLatestMessageToCondensedHistory("user", userAction.action);

                currentEventState = EventState.Narrating;
                llmManager.ContinueNarration(userAction, narrationCount >= maxNarrationCount - 2,
                    userAction.satiety, userAction.progression);
            }
        }

        void RelaunchLLMRequest()
        {
            switch (currentEventState)
            {
                case EventState.Starting:
                    PrepareEvent();
                    break;
                case EventState.Narrating:
                    ProcessUserAction(lastPlayerAction, false);
                    break;
                case EventState.AwaitingAction:
                    llmManager.CreateAction(StripQuestionsFromNarration(lastNarration), currentEventHint, nbrActions);
                    break;
                case EventState.Ending:
                    ProcessUserAction(lastPlayerAction);
                    break;
                case EventState.CreatingItem:
                    CreateGenericItem();
                    break;
            }
        }

        #region HistoryManagement
        void AddLatestMessageToCondensedHistory(string role, string message)
        {
            llmManager.AddToNarrationHistory(role, message);
        }
        #endregion

        protected override void LoseGame()
        {
            base.LoseGame();
            llmManager.Stop();
        }

        protected override void WinGame()
        {
            base.WinGame();
            llmManager.Stop();
        }

        void EndEventMode()
        {
            EndGameMode();
        }

        public override void RestartGameMode()
        {
            base.RestartGameMode();
            repeatabilityHandler.Clear(); 
        }

        public async override void DisableGameMode()
        {
            base.DisableGameMode();
            eventPanel.Clear();
            await llmManager.ResetNarration();
            llmManager.ClearNarrationHistory();
        }

        protected override void InterruptGameMode()
        {
            CallInteruptGameMode(new SavedDatas
            (
                inventoryManager.Items,
                satietyManager.CurrentSatiety,
                exitManager.CurrentProgress,
                true
            ));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (llmManager != null)
            {
                llmManager.OnAIStreaming -= HandleAIStreaming;
                llmManager.OnAIResponded -= HandleAIResponse;
                llmManager.OnAIProcessFailed -= HandleAIError;
            }

            if (eventPanel != null)
            {
                eventPanel.OnUserActionSelected -= SendUserActionToAI;
                eventPanel.OnConfirmClicked -= EndEventMode;
                eventPanel.OnRelaunchLLMProcessClicked -= RelaunchLLMRequest;
            }
        }


        #region JSON Handle 
        /// <summary>
        /// Extract the JSON response
        /// </summary>
        T SerializeAIResponse<T>(string jsonResponse) where T : class
        {
            try
            {
                string cleanedJson = JsonExtractorRegex.Match(jsonResponse).Value;
                return JsonUtility.FromJson<T>(cleanedJson);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to parse JSON narration: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Remove entirely the JSON to keep only the raw content.
        /// </summary>
        string CleanResponseFromJSON(string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput))
                return string.Empty;

            string result = MarkdownBlockRegex.Replace(rawInput, string.Empty);
            result = JsonStructureRegex.Replace(result, string.Empty);
            return CleanResponse(result);
        }

        string CleanResponse(string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput))
            {
                return string.Empty;
            }

            string result = char.ToUpperInvariant(rawInput[0]) + rawInput.Substring(1);
            result = result.Trim().Trim('"');
            result = result.Replace("\\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("_", " ");
            return MultiSpaceRegex.Replace(result, " ").Trim();
        }
        #endregion
    }
}
