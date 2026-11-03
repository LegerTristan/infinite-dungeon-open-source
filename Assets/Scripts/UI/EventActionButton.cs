using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class EventActionButton : BaseComponent
    {
        public event Action OnActionCalled = null;

        [SerializeField]
        RectTransform shakeAnimTransform = null;

        RectTransform zoomInAnimTransform = null;

        [Header("UI Components")]
        [SerializeField]
        Button actionButton = null;

        [SerializeField]
        TMP_Text actionText = null;

        [SerializeField]
        CanvasGroup canvasGroup = null;

        Vector2 defaultAnchorPos = Vector2.zero,
                targetAnchorPos = Vector2.zero;

        [Header("Shake Animation")]
        [SerializeField, Range(0.5f, 10f)]
        float animRadius = 1f;

        [SerializeField, Range(1f, 50f)]
        float animSpeed = 10f;

        [SerializeField]
        bool playShakeAnim = false;

        [Header("Zoom In Animation")]
        [SerializeField, Range(0.1f, 3f)]
        float zoomInDuration = 3f;

        float zoomInElapsed = 0f;

        [SerializeField]
        bool playZoomInAnim = true;

        bool areAnimsEnabled = true;

        protected override bool IsValid() => canvasGroup && shakeAnimTransform && actionButton && actionText;

        void Awake()
        {
            zoomInAnimTransform = (RectTransform)transform;
        }

        protected override void Start()
        {
            base.Start();

            defaultAnchorPos = shakeAnimTransform.anchoredPosition;
            actionButton.onClick.AddListener(CallAction);
            canvasGroup.alpha = 0f;
            actionButton.interactable = false;
        }

        public void DisplayAction()
        {
            actionText.text = string.Empty;
            zoomInElapsed = 0f;
            areAnimsEnabled = true;
            playZoomInAnim = true;
        }

        public void SetActionText(string text)
        {
            actionText.text = text;
        }

        public void SetActionButtonState(bool enable)
        {
            actionButton.interactable = enable;
        }

        public void HideAction()
        {
            canvasGroup.alpha = 0f;
            areAnimsEnabled = false;
        }

        void CallAction()
        {
            OnActionCalled?.Invoke();
        }

        void Update()
        {
            if (!areAnimsEnabled)
            {
                return;
            }

            if (playShakeAnim)
            {
                float distanceFromTarget = Vector2.Distance(shakeAnimTransform.anchoredPosition, targetAnchorPos);
                if (distanceFromTarget >= 0.01f)
                {
                    shakeAnimTransform.anchoredPosition = Vector2.MoveTowards(shakeAnimTransform.anchoredPosition, targetAnchorPos, Time.deltaTime * animSpeed);
                }
                else
                {
                    targetAnchorPos = defaultAnchorPos + (Random.insideUnitCircle * animRadius);
                }
            }

            if (playZoomInAnim)
            {
                zoomInElapsed += Time.deltaTime;
                float progress = zoomInDuration > 0f ? Mathf.Clamp01(zoomInElapsed / zoomInDuration) : 1f;
                zoomInAnimTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
                canvasGroup.alpha = progress;

                if (zoomInElapsed >= zoomInDuration)
                {
                    playZoomInAnim = false;
                }
            }
        }

        void OnDestroy()
        {
            if (actionButton != null)
            {
                actionButton.onClick.RemoveListener(CallAction);
            }
        }
    }

}