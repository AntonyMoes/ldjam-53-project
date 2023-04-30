using System;
using GeneralUtils;
using GeneralUtils.UI;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class UIController : SingletonBehaviour<UIController> {
        [SerializeField] private MainMenuWindow _mainMenuWindow;
        [SerializeField] private CreditsWindow _creditsWindow;
        [SerializeField] private ExitPanel _exitPanel;
        [SerializeField] private GameUIPanel _gameUIPanel;

        /*[SerializeField] private Transform _hider;
        [SerializeField] private Transform _windows;*/

        public MainMenuWindow ShowMainMenuWindow(Action startLevel = null) {
            if (startLevel != null) {
                _mainMenuWindow.Load(startLevel);
            }

            _mainMenuWindow.Show();
            return _mainMenuWindow;
        }

        public CreditsWindow ShowCreditsWindow() {
            _creditsWindow.Show();
            return _creditsWindow;
        }

        public ExitPanel ShowExitPanel(Action endLevel, Action restart = null) {
            _exitPanel.Load(endLevel, restart);
            _exitPanel.Show();
            return _exitPanel;
        }

        public GameUIPanel ShowGameUIPanel(UpdatedValue<float> patience, float maxPatience) {
            _gameUIPanel.Load(patience, maxPatience);
            _gameUIPanel.Show();
            return _gameUIPanel;
        }

        private void OnCancel() {
            GameController.Instance.OnCancel();

            if (_creditsWindow.State.Value == UIElement.EState.Shown) {
                _creditsWindow.OnBackClick();
            }
        }
        
        private void Update() {
            if (Input.GetButton("Cancel")) {
                OnCancel();
            }
        }

        /*private void PrepareWindow(UIElement window) {
            window.OnShowing.Unsubscribe(OnShowing);
            window.OnShowing.Subscribe(OnShowing);
            window.OnHiding.Unsubscribe(OnHiding);
            window.OnHiding.Subscribe(OnHiding);

            void OnShowing() {
                window.OnHiding.Unsubscribe(OnShowing);
                ShowWindow(window);
            }

            void OnHiding() {
                window.OnHiding.Unsubscribe(OnHiding);
                HideWindow(window);
            }
        }

        private void ShowWindow(UIElement window) {
            _hider.gameObject.SetActive(true);
            _hider.SetAsLastSibling();
            window.transform.SetAsLastSibling();
        }

        private void HideWindow(UIElement window) {
            UIElement lastActiveWindow = null;
            foreach (Transform child in _windows) {
                if (child.gameObject.activeSelf && child.TryGetComponent<UIElement>(out var activeWindow) && activeWindow != window)
                    lastActiveWindow = activeWindow;
            }

            if (lastActiveWindow != null) {
                _hider.SetSiblingIndex(lastActiveWindow.transform.GetSiblingIndex());
            } else {
                _hider.gameObject.SetActive(false);
            }
        }*/
    }
}