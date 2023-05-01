using System;
using System.Collections.Generic;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.Processes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts {
    public class TutorialController : MonoBehaviour {
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private CanvasGroup _tutorialGroup;
        [SerializeField] private RectTransform _mask;
        [SerializeField] private Button _nextStepButton;
        [SerializeField] private TextMeshProUGUI _stepText;
        [SerializeField] private RectTransform _stepTextTransform;
        [SerializeField] private GameObject _nextStepText;
        [SerializeField] private TutorialStep[] _steps;
        [SerializeField] private TutorialAction _exitAction;

        private TutorialStep[] _currentSteps;

        private const float AnimationDuration = 0.4f;
        private int _currentStep;
        private Action _onDone;

        private IDictionary<TutorialAction, Func<Process>> _actions;

        private void Awake() {
            _nextStepButton.onClick.AddListener(NextStep);
            _tutorialGroup.gameObject.SetActive(false);
        }

        public void LoadActions(IDictionary<TutorialAction, Func<Process>> actions) {
            _actions = actions;
        }

        public void StartTutorial(Action onDone = null) {
            StartTutorial(_steps, onDone);
        }

        private void StartTutorial(TutorialStep[] steps, Action onDone = null) {
            _currentSteps = steps;
            _currentStep = -1;
            _onDone = onDone;
            _tutorialGroup.alpha = 0f;
            _stepText.color = _stepText.color.WithAlpha(1f);
            NextStep();
        }

        private void NextStep() {
            if (++_currentStep >= _currentSteps.Length) {
                HideHider(() => GetActionProcess(_exitAction).Run(_onDone));
                return;
            }

            _nextStepButton.enabled = false;
            _nextStepText.SetActive(false);

            var stepProcess = new SerialProcess();
            stepProcess.Add(AsyncProcess.From(ShowHider, _currentStep == 0));
            stepProcess.Add(new LazyProcess(() => {
                var action = _currentSteps[_currentStep].action;
                return GetActionProcess(action);
            }));
            stepProcess.Add(new SyncProcess(() => {
                _nextStepText.SetActive(true);
                _nextStepButton.enabled = true;
            }));
            stepProcess.Run();
        }

        private Process GetActionProcess(TutorialAction action) {
            return action switch {
                TutorialAction.None => new DummyProcess(),
                _ => _actions[action]()
            };
        }

        private void ShowHider(bool initial, Action onDone) {
            var currentStep = _currentSteps[_currentStep];
            SetPositionSettings(_stepTextTransform, currentStep.position);

            var position = currentStep.mask.position;
            if (initial) {
                _mask.position = position;
                _mask.sizeDelta = currentStep.mask.sizeDelta;
                _stepText.text = currentStep.text;
                _stepTextTransform.anchoredPosition = Vector2.zero;
                _tutorialGroup.gameObject.SetActive(true);
                _tutorialGroup.DOFade(1f, AnimationDuration)
                    .OnComplete(() => onDone?.Invoke())
                    .SetUpdate(true);
                return;
            }

            DOTween.Sequence()
                .Insert(0f, _mask.DOMove(position, AnimationDuration))
                .Insert(0f, _mask.DOSizeDelta(currentStep.mask.sizeDelta, AnimationDuration))
                .Insert(0f, _stepTextTransform.DOAnchorPos(Vector2.zero, AnimationDuration))
                .Insert(0f, _stepText.DOFade(0f, AnimationDuration / 2f))
                .InsertCallback(AnimationDuration / 2f, UpdateText)
                .Insert(AnimationDuration / 2f, _stepText.DOFade(1f, AnimationDuration / 2f))
                .OnComplete(() => onDone?.Invoke())
                .SetUpdate(true);

            void UpdateText() {
                _stepText.text = currentStep.text;

                bool Has(TextPosition component) => HasComponent(currentStep.position, component);
                // var horAlignment = (Has(TextPosition.Left), Has(TextPosition.Right)) switch {
                //     (true, false) => TextAlignmentOptions.Right,
                //     (false, true) => TextAlignmentOptions.Left,
                //     _ => TextAlignmentOptions.Center
                // };
                var verAlignment = (Has(TextPosition.Up), Has(TextPosition.Down)) switch {
                    (true, false) => TextAlignmentOptions.Bottom,
                    (false, true) => TextAlignmentOptions.Top,
                    _ => TextAlignmentOptions.Midline
                };
                _stepText.alignment = verAlignment;
            }
        }

        private void HideHider(Action onDone) {
            _tutorialGroup.DOFade(0f, AnimationDuration).OnComplete(() => {
                _tutorialGroup.gameObject.SetActive(false);
                onDone?.Invoke();
            }).SetUpdate(true);
        }

        private static void SetPositionSettings(RectTransform textTransform, TextPosition position) {
            const float center = 0.5f;
            const float pivotDelta = 0.6f;

            var xPivot = center;
            var yPivot = center;
            var xAnchor = center;
            var yAnchor = center;

            bool Has(TextPosition component) => HasComponent(position, component);
            
            if (Has(TextPosition.Down)) {
                yPivot += pivotDelta;
                yAnchor -= center;
                // textTransform.pivot = new Vector2(center, center + pivotDelta);
                // textTransform.anchorMax = textTransform.anchorMin = new Vector2(center, 0f);
            }

            if (Has(TextPosition.Up)) {
                yPivot -= pivotDelta;
                yAnchor += center;
                // textTransform.pivot = new Vector2(center, center - pivotDelta);
                // textTransform.anchorMax = textTransform.anchorMin = new Vector2(center, 1f);
            }

            if (Has(TextPosition.Left)) {
                xPivot += pivotDelta;
                xAnchor -= center;
                // textTransform.pivot = new Vector2(center + pivotDelta, center);
                // textTransform.anchorMax = textTransform.anchorMin = new Vector2(0f, center);
            }

            if (Has(TextPosition.Right)) {
                xPivot -= pivotDelta;
                xAnchor += center;
                // textTransform.pivot = new Vector2(center - pivotDelta, center);
                // textTransform.anchorMax = textTransform.anchorMin = new Vector2(1f, center);
            }
            // } else {
            //     // case TextPosition.Center:
            //     //     textTransform.pivot = new Vector2(center, center);
            //     //     textTransform.anchorMax = textTransform.anchorMin = new Vector2(center, center);
            //     //     break;
            //     // throw new ArgumentOutOfRangeException(nameof(position), position, null);
            // }

            textTransform.pivot = new Vector2(xPivot, yPivot);
            textTransform.anchorMax = textTransform.anchorMin = new Vector2(xAnchor, yAnchor);
        }
        

        [Serializable]
        private struct TutorialStep {
            public RectTransform mask;
            [TextArea] public string text;
            public TutorialAction action;
            public TextPosition position;
        }

        public enum TutorialAction {
            None,
            Pause,
            Unpause
        }
        
        [Flags]
        private enum TextPosition {
            Center = 0,
            Down = 1 << 0,
            Up = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
            // Center = 1 << 4
        }

        private static bool HasComponent(TextPosition position, TextPosition component) => (position & component) == component;
    }
}