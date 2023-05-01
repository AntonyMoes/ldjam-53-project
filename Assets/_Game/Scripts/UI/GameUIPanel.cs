using System;
using _Game.Scripts.UI.Base;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class GameUIPanel : UIElement {
        [SerializeField] private RectTransform _characterPanel;
        [SerializeField] private ProgressBar _patienceProgressBar;
        [SerializeField] private TargetTimer _targetTimer;
        public TargetTimer TargetTimer => _targetTimer;

        [Header("Kills")]
        [SerializeField] private Color _flashColor;
        [SerializeField] private RectTransform _killsPanel;
        [SerializeField] private TextMeshProUGUI _killsLabel;

        [Header("Score")]
        [SerializeField] private RectTransform _scorePanel;
        [SerializeField] private TextMeshProUGUI _scoreLabel;

        private UpdatedValue<float> _patience;
        private UpdatedValue<int> _kills;
        private UpdatedValue<int> _score;
        private int _lastScore;
        private Tween _pbAnimation;
        private Tween _killsAnimation;
        private Tween _scoreAnimation;
        private Tween _panelAnimation;

        public void Load(UpdatedValue<float> patience, float maxPatience, UpdatedValue<int> kills, UpdatedValue<int> score) {
            _patienceProgressBar.Load(0, maxPatience);
            _patience = patience;
            _patience.Subscribe(OnPatienceUpdate);
            _patienceProgressBar.CurrentValue = _patience.Value;

            _kills = kills;
            _kills.Subscribe(OnKillsUpdate);
            _killsLabel.text = _kills.Value.ToString();

            _score = score;
            _score.Subscribe(OnScoreUpdate);
            _lastScore = _score.Value;
            _scoreLabel.text = _score.Value.ToString();
        }

        private void OnPatienceUpdate(float value) {
            _pbAnimation?.Kill();

            const float duration = 0.3f;
            var from = _patienceProgressBar.CurrentValue;
            _pbAnimation = DOVirtual.Float(from, value, duration, val => _patienceProgressBar.CurrentValue = val);
        }

        private void OnKillsUpdate(int value) {
            _killsAnimation?.Complete(true);

            _killsLabel.text = value.ToString();

            const float duration = 0.13f;
            var original = _killsLabel.color;
            _killsAnimation = DOTween.Sequence()
                .Append(_killsLabel.DOColor(_flashColor, duration).SetEase(Ease.OutSine))
                .Append(_killsLabel.DOColor(original, duration).SetEase(Ease.InSine));
        }

        private void OnScoreUpdate(int value) {
            _scoreAnimation?.Kill();

            const float duration = 0.8f;
            _scoreAnimation = DOVirtual.Int(_lastScore, value, duration, val => {
                _scoreLabel.text = val.ToString();
                _lastScore = val;
            });
        }

        public override void Clear() {
            _patience.Unsubscribe(OnPatienceUpdate);
            _kills.Unsubscribe(OnKillsUpdate);
            _score.Unsubscribe(OnScoreUpdate);
            _pbAnimation?.Kill();
        }

        protected override void PerformShow(Action onDone = null) {
            _panelAnimation?.Complete(true);

            const float duration = 0.3f;
            const float delay = 0.2f;

            var characterPanelTarget = _characterPanel.anchoredPosition;
            _characterPanel.anchoredPosition += Vector2.up * 2 * _characterPanel.sizeDelta;

            var patienceRect = (RectTransform) _patienceProgressBar.transform;
            var patiencePanelTarget = patienceRect.anchoredPosition;
            patienceRect.anchoredPosition += Vector2.left * 2 * patienceRect.sizeDelta;

            var killsPanelTarget = _killsPanel.anchoredPosition;
            _killsPanel.anchoredPosition += Vector2.down * 2 * _killsPanel.sizeDelta;

            var scorePanelTarget = _scorePanel.anchoredPosition;
            _scorePanel.anchoredPosition += Vector2.right * 2 * _scorePanel.sizeDelta;

            _panelAnimation = DOTween.Sequence()
                .Insert(0, _characterPanel.DOAnchorPos(characterPanelTarget, duration).SetEase(Ease.OutBack))
                .Insert(delay, patienceRect.DOAnchorPos(patiencePanelTarget, duration).SetEase(Ease.OutBack))
                .Insert(delay * 2, _killsPanel.DOAnchorPos(killsPanelTarget, duration).SetEase(Ease.OutBack))
                .Insert(delay * 3, _scorePanel.DOAnchorPos(scorePanelTarget, duration).SetEase(Ease.OutBack))
                .OnComplete(() => onDone?.Invoke());
        }

        protected override void PerformHide(Action onDone = null) {
            _panelAnimation?.Complete(true);
            _panelAnimation = null;
            base.PerformHide(onDone);
        }
    }
}