using System;
using System.Collections;
using UnityEngine;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeablePanel : BaseComponent
    {
        CanvasGroup canvasGroup = null;

        [SerializeField, Range(0f, 5f)]
        protected float fadeDuration = 1f;

        protected override bool IsValid() => canvasGroup;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void HideFade(Action callback = null)
        {
            StartCoroutine(FadeCanvasGroup(0f, callback));
        }

        public IEnumerator HideFadeCoroutine()
        {
            yield return FadeCanvasGroup(0f, null);
        }

        public void ShowFade(Action callback = null)
        {
            StartCoroutine(FadeCanvasGroup(1f, callback));
        }

        public IEnumerator ShowFadeCoroutine()
        {
            yield return FadeCanvasGroup(1f, null);
        }

        protected IEnumerator FadeCanvasGroup(float targetAlpha, Action callback = null)
        {
            if (!IsValid())
            {
                Debug.LogWarning("[FadeablePanel] => Component not valid ! CanvasGroup is missing !");
                yield break;
            }

            if (fadeDuration <= 0f)
            {
                canvasGroup.alpha = targetAlpha;
                callback?.Invoke();
                yield break;
            }

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            callback?.Invoke();
        }
    }
}