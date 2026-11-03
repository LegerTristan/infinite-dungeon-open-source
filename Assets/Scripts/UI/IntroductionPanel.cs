using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LingeringHopes.UI
{
    public class IntroductionPanel : FadeablePanel
    {
        [SerializeField]
        TMP_Text introText = null;

        [SerializeField]
        TMP_Text planeModeText = null;

        [SerializeField, Range(0.1f, 50f)]
        float planeModeDisplayDuration = 3f, introDisplayDuration = 3f;

        protected override bool IsValid() => base.IsValid() && planeModeText && introText;

        public Task DisplayIntroduction()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            StartCoroutine(IntroductionCoroutine(tcs));
            return tcs.Task;
        }

        IEnumerator IntroductionCoroutine(TaskCompletionSource<bool> tcs)
        {
            introText.alpha = 0f;
            planeModeText.alpha = 0f;

            StartCoroutine(FadeTextRoutine(planeModeText, 1f, fadeDuration));
            yield return FadeCanvasGroup(1f);
            yield return new WaitForSeconds(planeModeDisplayDuration);

            StartCoroutine(FadeTextRoutine(planeModeText, 0f, fadeDuration));
            yield return FadeTextRoutine(introText, 1f, fadeDuration);
            yield return new WaitForSeconds(introDisplayDuration);

            yield return FadeCanvasGroup(0f);
            tcs.TrySetResult(true);
        }

        IEnumerator FadeTextRoutine(TMP_Text text, float target, float duration)
        {
            if(duration <= 0f)
            {
                text.alpha = target;
                yield break;
            }

            float start = text.alpha;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            text.alpha = target;
        }

        public void Close()
        {
            if(planeModeText)
            {
                planeModeText.gameObject.SetActive(false);
            }

            if(introText)
            {
                introText.gameObject.SetActive(false);
            }
        }
    }
}