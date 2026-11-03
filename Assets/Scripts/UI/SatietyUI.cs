using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LingeringHopes.UI
{
    public class SatietyUI : BaseSlider
    {
        List<Image> tiredStackIcons = new List<Image>();

        [Header("Tired Stack Configuration")]
        [SerializeField]
        RectTransform tiredStackContent = null;

        [SerializeField]
        Image tiredStackIconPrefab = null;

        protected override bool IsValid() => base.IsValid() && tiredStackContent && tiredStackIconPrefab;

        public void SetTiredStack(uint tiredStack)
        {
            while (tiredStackIcons.Count < tiredStack)
            {
                Image newIcon = Instantiate(tiredStackIconPrefab, tiredStackContent);
                tiredStackIcons.Add(newIcon);
            }

            for (int i = 0; i < tiredStackIcons.Count; i++)
            {
                float targetAlpha = (i < tiredStack) ? 1f : 0f;
                tiredStackIcons[i].CrossFadeAlpha(targetAlpha, 0.01f, false);
            }
        }
    }
}