using System;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class ExitManager : MonoBehaviour
    {
        #region Variables
        public event Action<float> OnExitProgressChanged = null;
        public event Action OnExitReached = null;

        [SerializeField, Range(0, 1f)]
        float progressToReach = 1f;

        [SerializeField, Range(0, 1f)]
        float progressRate = .1f;

        float currentProgress = 0f;
        #endregion

        public float CurrentProgress => currentProgress;

        public float NormalizedProgress => progressToReach > 0 ? currentProgress / progressToReach : 0f;

        void Start()
        {
            SetCurrentProgress(0f);
        }

        public void ProgressThroughExit(float customProgressRate = -1f)
        {
            float rate = customProgressRate >= 0 ? customProgressRate : progressRate;
            SetCurrentProgress(currentProgress + rate);

            if (currentProgress >= progressToReach)
            {
                OnExitReached?.Invoke();
            }
        }

        public void Regress(float difference = -1f)
        {
            float finalDifference = difference <= 0f ? progressRate : difference;
            SetCurrentProgress(currentProgress - finalDifference);
        }

        public void Restart()
        {
            SetCurrentProgress(0f);
        }

        public void SetCurrentProgress(float progress)
        {
            currentProgress = Mathf.Clamp(progress, 0f, progressToReach);
            OnExitProgressChanged?.Invoke(NormalizedProgress);
        }
    }

}