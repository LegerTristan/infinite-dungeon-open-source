using LLMUnity;
using System;
using UnityEngine;


namespace LingeringHopes.LargeLanguageModel
{
    [Serializable]
    public class LLMSettings
    {
        [SerializeField]
        LLM llm = null;

        [SerializeField]
        float temperature = 2.0f;

        [SerializeField]
        int topK = 50;

        [SerializeField]
        float topP = 0.9f;

        [SerializeField]
        float frequencyPenalty = 0.8f;

        [SerializeField]
        float presencePenalty = 0.3f;

        [SerializeField]
        int seed = -1;

        [SerializeField]
        bool debug = false;

        public LLM LLM => llm;

        public float Temperature => temperature;

        public int TopK => topK;

        public float TopP => topP;

        public float FrequencyPenalty => frequencyPenalty;

        public float PresencePenalty => presencePenalty;

        public int Seed => seed;

        public bool Debug => debug;
    }
}