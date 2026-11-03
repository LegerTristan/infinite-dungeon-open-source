using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class ChatHistory : BaseComponent
    {
        [Header("UI References")]
        [SerializeField]
        ScrollRect historyScrollRect = null;

        [SerializeField]
        protected TMP_Text historyContentText = null;

        string baseText = null, lastPlaceholderText = null;

        bool isStreaming = false;

        protected override bool IsValid() => historyScrollRect && historyContentText;

        public void AddTextToHistory(string text, bool lastCurrentStream)
        {
            // Early return function if text is empty.
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            RemovePlaceholderText();
            //Debug.Log("Streaming token received: " + text);
            if (!isStreaming)
            {
                isStreaming = true;
                baseText = historyContentText.text;
            }

            historyContentText.text = $"{baseText}{text}\n\n";

            if (lastCurrentStream)
            {
                isStreaming = false;
            }

            // Auto scroll to the bottom.
            ScrollToBottom();
        }

        public void AddPlaceholderTextToHistory(string placeholder)
        {
            if (string.IsNullOrEmpty(placeholder))
            {
                return;
            }

            RemovePlaceholderText();
            lastPlaceholderText = placeholder;
            historyContentText.text += placeholder;
        }

        void RemovePlaceholderText()
        {
            if (!string.IsNullOrEmpty(lastPlaceholderText))
            {
                historyContentText.text = historyContentText.text.Replace(lastPlaceholderText, "");
                lastPlaceholderText = null;
            }
        }

        protected void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            historyScrollRect.verticalNormalizedPosition = 0;
            historyScrollRect.velocity = new Vector2(0f, 1000f);
        }

        public void Clear()
        {
            historyContentText.text = string.Empty;
            baseText = string.Empty;
            lastPlaceholderText = null;
            isStreaming = false;
        }
    }
}
