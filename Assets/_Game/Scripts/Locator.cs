using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class Locator : SingletonBehaviour<Locator> {
        [Header("Objects")]
        [SerializeField] private Camera _mainCamera;
        public Camera MainCamera => _mainCamera;

        [SerializeField] private Camera _uiCamera;
        public Camera UICamera => _uiCamera;

        [SerializeField] private Canvas _canvas;
        public Canvas Canvas => _canvas;

        [Header("Config")]
        [SerializeField] private GameConfig _config;
        public GameConfig Config => _config;
    }
}