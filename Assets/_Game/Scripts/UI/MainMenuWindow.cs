using System;
using _Game.Scripts.UI.Base;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class MainMenuWindow : GameUIElement {
        [SerializeField] private CanvasGroup _contents;
        [SerializeField] private BaseButton _startButton;
        [SerializeField] private BaseButton _settingsButton;

        private Action _startLevel;
        private Tween _tween;

        protected override void Init() {
            _startButton.OnClick.Subscribe(OnLevelStartClick);
            _settingsButton.OnClick.Subscribe(OnCreditsClick);
        }

        public void Load(Action startLevel) {
            _startLevel = startLevel;
        }

        private void OnLevelStartClick() {
            Hide(() => _startLevel());
        }

        private void OnCreditsClick() {
            Hide(() => UIController.Instance.ShowCreditsWindow());
        }

        protected override void PerformShow(Action onDone = null) {
            _tween?.Complete(true);

            const float duration = 0.3f;

            _contents.alpha = 0f;

            _tween = DOTween.Sequence() 
                .Insert(0f, _contents.DOFade(1f, duration))
                .AppendCallback(() => {
                    onDone?.Invoke();
                });
        }

        protected override void PerformHide(Action onDone = null) {
            _tween?.Complete(true);

            const float duration = 0.3f;

            _tween = DOTween.Sequence()
                .Insert(0f, _contents.DOFade(0f, duration))
                .AppendCallback(() => {
                    _contents.alpha = 1f;
                    onDone?.Invoke();
                });
        }
    }
}