using System;
using UnityEngine;

namespace LingeringHopes.LargeLanguageModel
{
    [Serializable]
    public struct SatietyPromptPair
    {
        [SerializeField, TextArea(2, 4)] 
        string prompt;

        [SerializeField]  
        float minSatietyValue;

        public string Prompt => prompt;

        public float MinSatietyValue => minSatietyValue;
    }
}
