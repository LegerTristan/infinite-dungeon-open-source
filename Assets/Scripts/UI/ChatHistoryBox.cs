using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LingeringHopes.Gameplay;

namespace LingeringHopes.UI
{
    public class ChatHistoryBox : BaseComponent
    {
        [SerializeField]
        TMP_Text contentText = null;

        [SerializeField]
        ContentSizeFitter sizeFitter = null;

        protected override bool IsValid() => contentText && sizeFitter;

        public void Initialize(string instigatorName, string text)
        {
            string instigatorNamePrefixe = string.IsNullOrEmpty(instigatorName) ? string.Empty : $"{instigatorName}: ";
            contentText.text = instigatorNamePrefixe + text;
        }

        public void UpdateText(string text)
        {
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentText.text = text;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(sizeFitter.GetComponent<RectTransform>());
        }
    }
}