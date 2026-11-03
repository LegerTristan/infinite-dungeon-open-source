using System.Collections;
using TMPro;
using UnityEngine;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ConsequencePanel : BaseComponent
    {
        RectTransform panel = null;

        [Header("UI References")]
        [SerializeField]
        TMP_Text consequenceText = null;

        Coroutine currentAnimation = null;

        [Header("Animation Settings")]
        [SerializeField]
        Vector3 translation = Vector3.zero;

        Vector3 defaultPos = Vector3.zero;

        [SerializeField, Range(0.01f, 5f)]
        float translationDuration = 1f, notifyDuration = 3f;

        protected override bool IsValid() => panel && consequenceText;

        void Awake()
        {
            panel = GetComponent<RectTransform>();
            defaultPos = panel.anchoredPosition;
        }

        public void ShowConsequence(string notification, Color notifcationColor)
        {
            consequenceText.text = notification;
            consequenceText.color = notifcationColor;

            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }

            currentAnimation = StartCoroutine(PlayTranslationAnimationRoutine());
        }

        IEnumerator TranslateRoutine(Vector3 targetPos)
        {
            float elapsed = 0f;
            Vector3 startingPos = panel.anchoredPosition;

            while (elapsed < translationDuration)
            {
                elapsed += Time.deltaTime;
                float percent = Mathf.Clamp01(elapsed / translationDuration);
                float curve = Mathf.SmoothStep(0, 1, percent);

                panel.anchoredPosition = Vector3.Lerp(startingPos, targetPos, curve);
                yield return null;
            }

            panel.anchoredPosition = targetPos;

        }

        IEnumerator PlayTranslationAnimationRoutine()
        {
            yield return TranslateRoutine(defaultPos + translation);
            yield return new WaitForSeconds(notifyDuration);
            yield return TranslateRoutine(defaultPos);
            currentAnimation = null;
        }
    }
}
