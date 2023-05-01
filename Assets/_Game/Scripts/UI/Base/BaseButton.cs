using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Base {
    public class BaseButton : MonoBehaviour {
        [SerializeField] private Button _button;
        [SerializeField] private string _sfx;

        private readonly Action _onClick;
        public readonly GeneralUtils.Event OnClick;

        public bool Interactable {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public BaseButton() {
            OnClick = new GeneralUtils.Event(out _onClick);
        }

        protected virtual void Awake() {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick() {
            SoundController.Instance.PlaySound(_sfx, 0.3f);
            _onClick();
        }
    }
}