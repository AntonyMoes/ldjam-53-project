using GeneralUtils;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Base {
    public class HoverButton : BaseButton {
        [SerializeField] private Image _hoverZone;

        private readonly UpdatedValue<bool> _onHover = new UpdatedValue<bool>(false);
        public IUpdatedValue<bool> OnHover => _onHover;

        protected override void Awake() {
            base.Awake();

            if (_hoverZone != null) {
                HoverComponent.Create(_hoverZone, _onHover);
            }
        }
    }
}