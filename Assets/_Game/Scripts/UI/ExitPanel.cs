using System;
using _Game.Scripts.UI.Base;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class ExitPanel : GameUIElement {
        [SerializeField] private CanvasGroup _contents;
        [SerializeField] private BaseButton _restartButton;
        [SerializeField] private BaseButton _continueButton;
        [SerializeField] private BaseButton _mainMenuButton;

        [Header("Text")]
        [SerializeField] private string _leavingText;
        [SerializeField] private string _lostText;
        [SerializeField] private TextMeshProUGUI _text;

        [Header("LoseContent")]
        [SerializeField] private GameObject _loseContent;
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private TextMeshProUGUI _orders;

        public bool Lost => _restartLevel != null;

        private Action _restartLevel;
        private Action _endLevel;
        private Tween _tween;

        protected override void Init() {
            _restartButton.OnClick.Subscribe(OnRestartClick);
            _continueButton.OnClick.Subscribe(OnContinueClick);
            _mainMenuButton.OnClick.Subscribe(OnMainMenuClick);

            var timeScale = Time.timeScale;
            OnShown.Subscribe(() => {
                if (!Lost) {
                    timeScale = Time.timeScale;
                    Time.timeScale = 0f;
                }
            });
            OnHiding.Subscribe(() => {
                if (!Lost) {
                    Time.timeScale = timeScale;
                }
            });
        }

        public void Load(Action endLevel, Action restartLevel = null, int score = 0, int orders = 0) {
            _endLevel = endLevel;
            _restartLevel = restartLevel;

            _restartButton.gameObject.SetActive(Lost);
            _continueButton.gameObject.SetActive(!Lost);

            _text.text = Lost ? _lostText : _leavingText;
            _loseContent.SetActive(Lost);
            _score.text = score.ToString();
            _orders.text = orders.ToString();
        }

        private void OnRestartClick() {
            Hide(() => _restartLevel?.Invoke());
        }

        private void OnMainMenuClick() {
            _endLevel?.Invoke();
            Hide(() => UIController.Instance.ShowMainMenuWindow());
        }

        private void OnContinueClick() {
            Hide();
        }

        protected override void PerformShow(Action onDone = null) {
            _tween?.Complete(true);

            const float duration = 0.3f;

            _contents.alpha = 0f;
            var ct = (RectTransform) _contents.transform;
            var initialPosition = ct.anchoredPosition;
            ct.anchoredPosition = initialPosition + Vector2.up * ct.sizeDelta * 0.5f;

            _tween = DOTween.Sequence()
                .Insert(0f, _contents.DOFade(1f, duration))
                .Insert(0f, ct.DOAnchorPos(initialPosition, duration))
                .AppendCallback(() => {
                    _contents.alpha = 1f;
                    ct.anchoredPosition = initialPosition;
                    onDone?.Invoke();
                });
        }

        protected override void PerformHide(Action onDone = null) {
            _tween?.Complete(true);

            const float duration = 0.3f;
            var ct = (RectTransform) _contents.transform;
            var initialPosition = ct.anchoredPosition;

            _tween = DOTween.Sequence()
                .Insert(0f, _contents.DOFade(0f, duration))
                .Insert(0f, ct.DOAnchorPos(initialPosition + Vector2.down * ct.sizeDelta * 0.5f, duration))
                .AppendCallback(() => {
                    _contents.alpha = 1f;
                    ct.anchoredPosition = initialPosition;
                    onDone?.Invoke();
                });
        }
    }
}