using System;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class SatietyManager : MonoBehaviour
    {
        #region Variables
        const float MAX_SATIETY = 1f;

        public event Action<float> OnSatietyChanged = null;
        public event Action<uint> OnTiredStackChanged = null;
        public event Action OnStarveToDeath = null;

        [Header("Metabolism Settings")]
        [SerializeField, Range(0.1f, 2f)]
        float baseMetabolismRate = 1f;

        [SerializeField, Range(0.1f, 1f)]
        float metabolismConsumption = 0.1f, starvingThreshold = 0.1f;

        float /*currentMetabolismRate = 1f, */satiety = 1f;

        uint tiredStack = 0;

        public float CurrentSatiety => satiety;

        float TiredRate => 1f + tiredStack;

        public uint TiredStack => tiredStack;

        public bool IsStarving => satiety <= starvingThreshold;
        #endregion

        void Start()
        {
            //urrentMetabolismRate = baseMetabolismRate;
            SetSatiety(satiety);
        }

        public void IncreaseSatiety(float gain = -1f)
        {
            float finalGain = gain <= 0f ? metabolismConsumption : gain;
            SetSatiety(satiety + finalGain);
        }

        public void ReduceSatiety(float difference = -1f)
        {
            float finalDifference = difference <= 0f ? metabolismConsumption : difference;
            SetSatiety(satiety - (finalDifference * TiredRate));
        }

        public void SetSatiety(float newSatiety)
        {
            satiety = Mathf.Clamp(newSatiety, 0f, MAX_SATIETY);
            OnSatietyChanged?.Invoke(satiety);

            if (satiety <= 0f)
            {
                OnStarveToDeath?.Invoke();
            }
        }

        #region MetabolismRate
        //public void UpdateMetabolismRate(float rate)
        //{
        //    currentMetabolismRate = rate;
        //}

        //public void ResetMetabolismRate()
        //{
        //    currentMetabolismRate = baseMetabolismRate;
        //}
        #endregion

        #region TiredEffect
        public void IncrementTiredStack()
        {
            tiredStack++;
            OnTiredStackChanged?.Invoke(tiredStack);
        }

        public void ResetTiredStack()
        {
            tiredStack = 0;
            OnTiredStackChanged?.Invoke(tiredStack);
        }
        #endregion

        public void Restart()
        {
            SetSatiety(MAX_SATIETY);
            ResetTiredStack();
        }
    }

}