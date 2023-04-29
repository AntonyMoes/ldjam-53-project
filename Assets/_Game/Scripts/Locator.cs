using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class Locator : SingletonBehaviour<Locator> {
        [SerializeField] private Camera _mainCamera;
        public Camera MainCamera => _mainCamera;

        [SerializeField] private Camera _uiCamera;
        public Camera UICamera => _uiCamera;

        [SerializeField] private Canvas _canvas;
        public Canvas Canvas => _canvas;
    }
}