using _Game.Scripts.UI.Base;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class AnchoredObjectProgressBar : ProgressBar {
        [SerializeField] private RectTransform _object;
        [SerializeField] private Vector2 _anchorMin;
        [SerializeField] private Vector2 _anchorMax;

        protected override void UpdateProgress(float progress) {
            var anchor = Vector2.Lerp(_anchorMin, _anchorMax, progress);
            _object.anchorMin = _object.anchorMax = anchor;
            _object.anchoredPosition = Vector2.zero;
        }
    }
}