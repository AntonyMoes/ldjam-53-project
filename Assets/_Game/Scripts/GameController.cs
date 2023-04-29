using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Objects;
using _Game.Scripts.UI.Base;
using DG.Tweening;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class GameController : SingletonBehaviour<GameController> {
        [Header("Game UI")]
        [SerializeField] private ProgressBar _patienceProgressBar;

        [Header("Objects")]
        [SerializeField] private Player _player;
        
        [Header("Config")]
        [SerializeField] private GameConfig _config;

        private readonly UpdatedValue<float> _patience;
        private List<Pedestrian> _pedestrians;
        private Rng _rng;

        public GameController() {
            _patience = new UpdatedValue<float>(setter: SetPatience);
        }

        private void Start() {
            _rng = new Rng(Rng.RandomSeed);

            _pedestrians = FindObjectsOfType<Pedestrian>().ToList();
            foreach (var pedestrian in _pedestrians) {
                pedestrian.OnCollision.Subscribe(OnPedestrianCollision);
            }

            SetTarget();

            _patience.Value = _config.InitialPatience;
            _patienceProgressBar.Load(0, _config.MaxPatience);
            _patience.Subscribe(val => _patienceProgressBar.CurrentValue = val, true);
        }

        private void OnPedestrianCollision(Pedestrian pedestrian) {
            pedestrian.OnCollision.Unsubscribe(OnPedestrianCollision);
            _pedestrians.Remove(pedestrian);

            // TODO
            if (pedestrian.IsTarget) {
                Debug.LogError("Nice!");
                pedestrian.IsTarget = false;

                _patience.Value += _config.PatienceOnSuccess;
                SetTarget();
            } else {
                Debug.LogError("Wrong!");
                _patience.Value -= _config.PatienceOnMistake;
                if (_patience.Value == 0) {
                    Debug.LogError("GAME OVER");
                    Destroy(_player.gameObject);
                }
            }

            // TODO
            DOVirtual.DelayedCall(0.5f, () => Destroy(pedestrian.gameObject));
        }

        private void SetTarget() {
            if (_pedestrians.Count == 0) {
                return;
            }

            _rng.NextChoice(_pedestrians).IsTarget = true;
        }

        private float SetPatience(float value) => Mathf.Clamp(value, 0, _config.MaxPatience);
    }
}