using _Game.Scripts.UI.Base;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.UI;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class GameUIPanel : UIElement {
        [SerializeField] private ProgressBar _patienceProgressBar;

        private UpdatedValue<float> _patience;
        private Tween _pbAnimation;
        

        public void Load(UpdatedValue<float> patience, float maxPatience) {
            _patienceProgressBar.Load(0, maxPatience);
            _patience = patience;
            _patience.Subscribe(OnPatienceUpdate);
            _patienceProgressBar.CurrentValue = _patience.Value;
        }

        private void OnPatienceUpdate(float value) {
            _pbAnimation?.Kill();

            const float duration = 0.3f;
            var from = _patienceProgressBar.CurrentValue;
            _pbAnimation = DOVirtual.Float(from, value, duration, val => _patienceProgressBar.CurrentValue = val);
        }

        public override void Clear() {
            _patience.Unsubscribe(OnPatienceUpdate);
            _pbAnimation?.Kill();
        }
    }
}