using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class SlidesPanel : GameUIElement {
        [SerializeField] private CanvasGroup[] _slides;
        [SerializeField] private Button _nextSlideButton;
        [SerializeField] private CanvasGroup _slidesGroup;
        [SerializeField] private CanvasGroup _loadingScreen;

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

            _loadingScreen.alpha = 1f;
            _loadingScreen.gameObject.SetActive(true);

            var hasNext = HasNext;
            DOTween.Sequence()
                .AppendCallback(() => {
                    if (hasNext) {
                        ShowNextSlide();
                    }
                })
                .AppendInterval(3f)
                .Append(_loadingScreen.DOFade(0f, 0.5f))
                .OnComplete(() => {
                    _loadingScreen.gameObject.SetActive(false);
                    _loadingScreen.alpha = 1f;
                    onDone?.Invoke();
                    if (!hasNext) {
                        Hide();
                    }
                });
        }

        private void NextSlide() {
            if (!HasNext) {
                Hide();
                return;
            }

            ShowNextSlide();
        }

        private void ShowNextSlide(Action onDone = null) {
            const float duration = 0.5f;

            _nextSlideButton.enabled = false;
            var hasCurrent = HasCurrent;
            var hasNext = HasNext;
            _currentSlideIdx++;

            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => {
                _slidesGroup.alpha = 0f;
            });

            if (hasNext) {
                var insertTime = /*hasCurrent ? duration * 0.25f : */0f;
                var next = CurrentSlide;
                sequence.InsertCallback(insertTime, () => {
                    next.alpha = 0f;
                    next.gameObject.SetActive(true);
                });
                var fadeInDuration = hasCurrent ? duration / 2f : 0f;
                sequence.Insert(insertTime, DOTween.Sequence()
                    .Insert(0, next.DOFade(1, fadeInDuration))
                    .Insert(0, _slidesGroup.DOFade(1, fadeInDuration)));
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