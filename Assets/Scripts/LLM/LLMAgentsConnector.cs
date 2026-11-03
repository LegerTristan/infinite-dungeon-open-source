using UnityEngine;

namespace LingeringHopes.LargeLanguageModel
{
    public class LLMAgentsConnector : MonoBehaviour
    {
        [SerializeField]
        NarrationAgent narrationAgent = null;

        [SerializeField]
        LogicAgent logicAgent = null;

        [SerializeField]
        ItemAgent itemAgent = null;

        [SerializeField]
        LLMManager llmManager = null;

        public bool IsValid => narrationAgent && logicAgent && itemAgent && llmManager;

        void Awake()
        {
            if (!IsValid)
            {
                Debug.LogError($"[LLMAgentsConnector] Missing agent or LLMManager references on {gameObject.name}");
                return;
            }

            llmManager.RegisterNarrationAgent(narrationAgent);
            llmManager.RegisterLogicAgent(logicAgent);
            llmManager.RegisterItemAgent(itemAgent);
        }
    }
}