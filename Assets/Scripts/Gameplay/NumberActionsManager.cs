using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class NumberActionsManager : BaseComponent
    {
        [Header("Probability Configuration")]
        [Tooltip("Index 0 = 2 actions, Index 1 = 3 actions, etc.")]
        [SerializeField]
        AnimationCurve[] actionWeights;

        float[] weights = null;

        [SerializeField]
        int minActions = 2;

        protected override bool IsValid() => actionWeights != null && actionWeights.Length > 0;

        protected override void Start()
        {
            base.Start();

            weights = new float[actionWeights.Length];
        }

        public int GetNumberActions(float exitProgress)
        {
            if (actionWeights == null || actionWeights.Length == 0)
                return minActions;

            float totalWeight = 0;
            for (int i = 0; i < actionWeights.Length; i++)
            {
                weights[i] = actionWeights[i].Evaluate(exitProgress);
                totalWeight += weights[i];
            }

            if (totalWeight <= 0f)
                return minActions;

            float randomValue = Random.Range(0, totalWeight);
            float currentSum = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                currentSum += weights[i];
                if (randomValue <= currentSum)
                {
                    return i + minActions;
                }
            }

            return minActions;
        }
    }
}