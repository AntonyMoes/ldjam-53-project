using _Game.Scripts.UI.Base;
using GeneralUtils;
using GeneralUtils.UI;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class GameUIPanel : UIElement {
        [SerializeField] private ProgressBar _patienceProgressBar;

        private UpdatedValue<float> _patience;

        public void Load(UpdatedValue<float> patience, float maxPatience) {
            _patienceProgressBar.Load(0, maxPatience);
            _patience = patience;
            _patience.Subscribe(OnPatienceUpdate, true);
        }

        private void OnPatienceUpdate(float value) => _patienceProgressBar.CurrentValue = value;

        public override void Clear() {
            _patience.Unsubscribe(OnPatienceUpdate);
        }
    }
}