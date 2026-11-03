using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class BaseSlider : BaseComponent
    {
        [Header("UI References")]
        [SerializeField]
        Slider slider = null;

        [SerializeField, Range(0.1f, 10f)]
        float animSpeed = 1f;

        float targetPercentage = 0f;

        protected override bool IsValid() => slider;

        public void UpdateSliderBar(float percentage, bool immediateUpdate = false)
        {
            if (!IsValid())
            {
                Debug.LogError("[BaseSlider] => Component not valid. Please check the inspector.");
                return;
            }

            targetPercentage = Mathf.Clamp01(percentage);
            if (immediateUpdate)
            {
                slider.value = targetPercentage;
            }
        }

        void Update()
        {
            if (slider != null && !Mathf.Approximately(slider.value, targetPercentage))
            {
                slider.value = Mathf.MoveTowards(slider.value, targetPercentage, animSpeed * Time.deltaTime);
            }
        }
    }

}