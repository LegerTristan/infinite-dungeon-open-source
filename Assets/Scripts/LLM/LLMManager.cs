using LingeringHopes.Gameplay;
using LLMUnity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LingeringHopes.LargeLanguageModel
{
    [RequireComponent(typeof(LLM))]
    public class LLMManager : BaseComponent, ILLMProvider
    {
        #region Variables
        public event Action<string, string> OnRequestStarted = null;
        public event Action<string> OnAIResponded = null;
        public event Action<string> OnAIStreaming = null;
        public event Action OnAIProcessFailed = null;

        readonly List<ChatMessage> narrationHistory = new List<ChatMessage>();

        LLM llm = null;

        ILLMAgent narrationAgent = null,
                  logicAgent = null,
                  itemAgent = null;

        [Header("LLM Configuration")]
        [SerializeField]
        string npcName = null;

        [SerializeField]
        string loraNarration = null, loraLogic = null, loraItem = null;

        string narrationState = string.Empty;

        bool enableLLM = true;
        #endregion

        public string GetModelName() => llm != null ? llm.model : string.Empty;

        protected override bool IsValid() => narrationAgent != null && logicAgent != null && !string.IsNullOrEmpty(npcName) && !string.IsNullOrEmpty(loraNarration)
            && !string.IsNullOrEmpty(loraLogic) && !string.IsNullOrEmpty(loraItem);

        #region AgentRegistration
        public void RegisterNarrationAgent(ILLMAgent agent)
        {
            if (narrationAgent != null)
            {
                narrationAgent.OnAgentProcessing -= RequestStartedForNarrationAgent;
            }

            narrationAgent = agent;
            if (narrationAgent != null)
            {
                narrationAgent.OnAgentProcessing += RequestStartedForNarrationAgent;
            }
        }

        public void RegisterLogicAgent(ILLMAgent agent)
        {
            if (logicAgent != null)
            {
                logicAgent.OnAgentProcessing -= RequestStartedForLogicAgent;
            }

            logicAgent = agent;
            if (logicAgent != null)
            {
                logicAgent.OnAgentProcessing += RequestStartedForLogicAgent;
            }
        }

        public void RegisterItemAgent(ILLMAgent agent)
        {
            if (itemAgent != null)
            {
                itemAgent.OnAgentProcessing -= RequestStartedForItemAgent;
            }

            itemAgent = agent;
            if(itemAgent != null)
            {
                itemAgent.OnAgentProcessing += RequestStartedForItemAgent;
            }
        }
        #endregion

        public void EnableLoraNarration()
        {
            llm.SetLoraWeight(loraNarration, 1);
            llm.SetLoraWeight(loraLogic, 0);
            llm.SetLoraWeight(loraItem, 0);
        }

        public void EnableLoraLogic()
        {
            llm.SetLoraWeight(loraNarration, 0);
            llm.SetLoraWeight(loraLogic, 1);
            llm.SetLoraWeight(loraItem, 0);
        }

        void EnableLoraItem()
        {
            llm.SetLoraWeight(loraNarration, 0);
            llm.SetLoraWeight(loraLogic, 0);
            llm.SetLoraWeight(loraItem, 1);
        }

        #region Setup
        void Awake()
        {
            llm = GetComponent<LLM>();
        }

        public async Task<bool> InitLLMServer(Action<float> progressCallback)
        {
            if (!IsValid())
            {
                Debug.LogError("[LLMManager] => Component not valid ! Something is missing !");
                enabled = false;
                return false;
            }

            bool result = await LLM.WaitUntilModelSetup(progressCallback);
            if (!result)
            {
                return false;
            }

            return await EnsureLLMReady();
        }

        async Task<bool> EnsureLLMReady()
        {
            if (!llm.started)
            {
                await llm.WaitUntilReady();
            }

            return llm.started;
        }
        #endregion

        #region WarmUp
        async Task AgentWarmUp(ILLMAgent agent, Action callbackOnWarmupDone = null)
        {
            if (agent == null || agent.IsWarmedUp())
            {
                return;
            }

            await agent.Warmup(callbackOnWarmupDone);
        }

        public async Task FullWarmup(Action<string, float> onProgress = null, Action onComplete = null)
        {
            while (!llm.started)
            {
                onProgress?.Invoke("Waiting for LLM engine to boot", 0.05f);
                await Task.Yield();
            }

            onProgress?.Invoke("Warming up Narration Agent", 0.15f);
            await NarrationWarmUp();

            onProgress?.Invoke("Warming up Logic Agent", 0.50f);
            await LogicWarmUp();

            onProgress?.Invoke("Warming up Item Agent", 0.85f);
            await ItemWarmUp();

            onProgress?.Invoke("Ready!", 1.0f);
            await Task.Delay(300); 

            onComplete?.Invoke();
        }

        public async Task NarrationWarmUp(Action callbackOnWarmupDone = null) => await AgentWarmUp(narrationAgent, callbackOnWarmupDone);

        public async Task LogicWarmUp(Action callbackOnWarmupDone = null) => await AgentWarmUp(logicAgent, callbackOnWarmupDone);

        public async Task ItemWarmUp(Action callbackOnWarmupDone = null) => await AgentWarmUp(itemAgent, callbackOnWarmupDone);
        #endregion

        #region LLMProcess
        #region NarrationAgent
        public async Task<string> NarrateStartEvent(EnvironmentTag envTag, EventTag evtTag, string eventHint, float playerSatiety, 
            bool playerStarving)
        {
            if (!enableLLM || narrationAgent == null || !narrationAgent.IsWarmedUp())
            {
                return null;
            }

            EnableLoraNarration();
            if (narrationAgent.IsWarmedUp())
            {
                narrationState = "Start_Narration";
                narrationAgent.CancelRequest();
                return await narrationAgent.Chat(
                    new NarrationStartPromptParameters(envTag, evtTag, playerSatiety, playerStarving),
                    OnDiagenChunkReceived
                );
            }

            return null;
        }

        public async Task ContinueNarration(ActionResponse response, bool isRunningOutOfTime, string satietyChange, string progressionChange)
        {
            if (!enableLLM || narrationAgent == null || !narrationAgent.IsWarmedUp())
            {
                return;
            }

            EnableLoraNarration();
            narrationState = "Continue_Narration";
            narrationAgent.CancelRequest();
            await narrationAgent.Chat(
                new NarrationContinuePromptParameters(response, satietyChange, progressionChange, isRunningOutOfTime),
                OnDiagenChunkReceived
            );
        }

        public async Task NarrateEndEvent(ActionResponse userAction, bool runnedOutOfTime)
        {
            if (!enableLLM || narrationAgent == null || !narrationAgent.IsWarmedUp())
            {
                return;
            }

            EnableLoraNarration();
            narrationState = "End_Narration";
            narrationAgent.CancelRequest();
            await narrationAgent.Chat(
                new NarrationEndingPromptParameters(userAction, runnedOutOfTime),
                OnDiagenChunkReceived
            );  
        }

        public void AddToNarrationHistory(string role, string message)
        {
            narrationHistory.Add(new ChatMessage(role, message));
            narrationAgent?.UpdateHistory(narrationHistory);
        }

        public void ClearNarrationHistory()
        {
            narrationHistory.Clear();
            narrationAgent?.UpdateHistory(narrationHistory);
        }
        #endregion

        #region LogicAgent
        public async Task CreateAction(string narration, string eventHint, int nbrActions)
        {
            if (!enableLLM || logicAgent == null || !logicAgent.IsWarmedUp())
            {
                return;
            }

            EnableLoraLogic();
            await logicAgent.Chat(new LogicPromptParameters(narration, eventHint, nbrActions), OnDiagenChunkReceived);
        }
        #endregion

        #region ItemAgent
        public async Task CreateItem(string itemName)
        {
            if (!enableLLM || itemAgent == null || !itemAgent.IsWarmedUp())
            {
                return;
            }

            EnableLoraItem();
            string itemResponse = await itemAgent.Chat(new ItemPromptParameters(itemName), OnDiagenChunkReceived);
        }
        #endregion

        async void OnDiagenChunkReceived(string chunk, bool hasEnded, bool errorOccured)
        {
            if (errorOccured)
            {
                OnAIProcessFailed?.Invoke();
                await Restart();
                return;
            }

            if (!hasEnded)
            {
                OnAIStreaming?.Invoke(chunk); // Stream chunk
                return;
            }

            //Debug.Log($"Response : {chunk}");
            OnAIResponded?.Invoke(chunk);
        }
        #endregion

        public void Stop()
        {
            if(narrationAgent != null)
            {
                narrationAgent.CancelRequest();
            }

            if(logicAgent != null)
            {
                logicAgent.CancelRequest();
            }

            if(itemAgent != null)
            {
                itemAgent.CancelRequest();
            }
        }

        public async Task Restart()
        {
            await narrationAgent.Reset();
            await logicAgent.Reset();
            await itemAgent.Reset();
        }

        #region HistoryManagement
        public async Task ResetNarration()
        {
            if (narrationAgent != null)
            {
                await narrationAgent.Reset();
            }
        }

        public void UpdateNarrationHistory(List<ChatMessage> newHistory)
        {
            if (narrationAgent != null)
            {
                narrationAgent.UpdateHistory(newHistory);
            }
        }
        #endregion

        void RequestStartedForNarrationAgent(string fullPrompt)
        {
            Debug.Log($"[LLMManager] => RequestStartedForNarrationAgent with {narrationState}");
            OnRequestStarted?.Invoke(fullPrompt, narrationState);
        }

        void RequestStartedForLogicAgent(string fullPrompt)
        {
            Debug.Log($"[LLMManager] => RequestStartedForLogicAgent");
            OnRequestStarted?.Invoke(fullPrompt, "Create_Action");
        }

        void RequestStartedForItemAgent(string fullPrompt)
        {
            Debug.Log($"[LLMManager] => RequestStartedForItemAgent");
            OnRequestStarted?.Invoke(fullPrompt, "Create_Item");
        }

        void OnDestroy()
        {
            Stop();
            CleanupRAM();
        }

        public void CleanupRAM()
        {
            llm.Destroy();
            llm.enabled = false;
            Resources.UnloadUnusedAssets();
        }
    }

}