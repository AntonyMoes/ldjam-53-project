using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class SlidesPanel : GameUIElement {
        [SerializeField] private CanvasGroup[] _slides;
        [SerializeField] private Button _nextSlideButton;
        [SerializeField] private CanvasGroup _slidesGroup;

        private int _currentSlideIdx;
        private CanvasGroup CurrentSlide => _slides[_currentSlideIdx];
        private bool HasCurrent => _currentSlideIdx >= 0;
        private bool HasNext => _currentSlideIdx < _slides.Length - 1;

        protected override void Init() {
            _nextSlideButton.onClick.AddListener(NextSlide);
        }

        protected override void PerformShow(Action onDone = null) {
            _currentSlideIdx = -1;
            foreach (var slide in _slides) {
                slide.gameObject.SetActive(false);
            }

            if (!HasCurrent && !HasNext) {
                onDone?.Invoke();
                Hide();
                return;
            }

            ShowNextSlide(onDone);
        }

        public void NextSlide() {
            if (!HasNext) {
                Hide();
                return;
            }

            ShowNextSlide();
        }

        private void ShowNextSlide(Action onDone = null) {
            const float duration = 0.8f;

            _nextSlideButton.enabled = false;

            var sequence = DOTween.Sequence();
            if (HasCurrent) {
                var current = CurrentSlide;
                sequence.Append(DOTween.Sequence()
                    .Insert(0, current.DOFade(0, duration / 2f))
                    .Insert(0, _slidesGroup.DOFade(0, duration / 2f)));
                sequence.AppendCallback(() => {
                    current.gameObject.SetActive(false);
                    current.alpha = 1f;
                });
            }

            var hasNext = HasNext;
            _currentSlideIdx++;
            sequence.AppendCallback(() => {
                _slidesGroup.alpha = 0f;
            });

            if (hasNext) {
                var next = CurrentSlide;
                sequence.AppendCallback(() => {
                    next.alpha = 0f;
                    next.gameObject.SetActive(true);
                });
                sequence.Append(DOTween.Sequence()
                    .Insert(0, next.DOFade(1, duration / 2f))
                    .Insert(0, _slidesGroup.DOFade(1, duration / 2f)));
            }

            sequence.OnComplete(() => {
                _nextSlideButton.enabled = true;
                onDone?.Invoke();
            });
        }

        protected override void PerformHide(Action onDone = null) {
            ShowNextSlide(onDone);
        }
    }
}