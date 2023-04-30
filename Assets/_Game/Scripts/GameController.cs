using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Objects;
using _Game.Scripts.Objects.Pedestrian;
using _Game.Scripts.UI;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.UI;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts {
    public class GameController : SingletonBehaviour<GameController> {
        [Header("UI")]
        [SerializeField] private TargetTimer _targetTimer;

        [Header("Objects")]
        [SerializeField] private GameObject _map;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private Pedestrian _pedestrianPrefab;
        [SerializeField] private Transform _pedestrianParent;
        [SerializeField] private MeshCollider _mapPlane;

        private MainMenuWindow _mainMenu;
        private GameUIPanel _gameUIPanel;
        private ExitPanel _exitPanel;

        public Player Player { get; private set; }

        private readonly UpdatedValue<float> _patience;
        private List<Pedestrian> _pedestrians;
        private Rng _rng;

        private bool LevelInProgress => _map.activeSelf;
        private readonly UpdatedValue<float> _timer = new UpdatedValue<float>();
        private Pedestrian _currentTarget;
        private Tween _timerTween;
        private bool _lost;

        public GameController() {
            _patience = new UpdatedValue<float>(setter: SetPatience);
        }

        private float SetPatience(float value) => Mathf.Clamp(value, 0, Locator.Instance.Config.MaxPatience);

        private void Start() {
            _mainMenu = UIController.Instance.ShowMainMenuWindow(StartLevel);
        }

        private void StartLevel() {
            _map.SetActive(true);
            _rng = new Rng(Rng.RandomSeed);

            Player = Instantiate(_playerPrefab, _playerSpawn);
            _cameraController.Target = Player.transform;

            var config = Locator.Instance.Config;
            _pedestrians = Enumerable.Range(0, config.Population).Select(_ => SpawnPedestrian()).ToList();
            SetTarget();

            _patience.Value = config.InitialPatience;
            _patience.WaitFor(0f, OnLose);

            _gameUIPanel = UIController.Instance.ShowGameUIPanel(_patience, config.MaxPatience);
        }

        private void EndLevel(Action onDone) {
            StopTimer();
            if (Player != null) {
                Destroy(Player.gameObject);
            }

            _map.SetActive(false);
            _patience.Clear();

            foreach (var pedestrian in _pedestrians) {
                pedestrian.Kill(true);
            }

            _gameUIPanel.Hide(onDone);
        }

        private void RestartLevel() {
            EndLevel(StartLevel);
        }

        private void EndLevelAndLeave() {
            EndLevel(() => _mainMenu.Show());
        }

        private void OnPedestrianCollision(Pedestrian pedestrian) {
            pedestrian.OnCollision.Unsubscribe(OnPedestrianCollision);
            pedestrian.Kill();
            _pedestrians.Remove(pedestrian);
            _pedestrians.Add(SpawnPedestrian());

            var config = Locator.Instance.Config;
            if (pedestrian.IsTarget) {
                Debug.LogError("Nice!");
                _patience.Value += config.PatienceOnSuccess;
                SetTarget();
            } else {
                Debug.LogError("Wrong!");
                _patience.Value -= config.PatienceOnMistake;
            }
        }

        private void OnTimerEnd() {
            Debug.LogError("Late!");
            _patience.Value -= Locator.Instance.Config.PatienceOnFail;

            if (_lost) {
                return;
            }

            SetTarget();
        }

        private void OnLose() {
            Debug.LogError("GAME OVER");
            _lost = true;
            StopTimer();
            Destroy(Player.gameObject);

            _exitPanel = UIController.Instance.ShowExitPanel(EndLevelAndLeave, RestartLevel);
        }

        public void OnCancel() {
            if (!LevelInProgress) {
                return;
            }
            
            if (_exitPanel == null || _exitPanel.State.Value == UIElement.EState.Hided) {
                _exitPanel = UIController.Instance.ShowExitPanel(EndLevelAndLeave);
            } else if (_exitPanel.State.Value == UIElement.EState.Shown && !_exitPanel.Lost) {
                _exitPanel.Hide();
            }
        }

        private void SetTarget() {
            StopTimer();
            if (_currentTarget != null) {
                _currentTarget.IsTarget = false;
                _currentTarget = null;
            }

            if (_pedestrians.Count == 0) {
                return;
            }

            _currentTarget = _rng.NextChoice(_pedestrians);
            _currentTarget.IsTarget = true;
            StartTimer();
        }

        private void StartTimer() {
            var duration = Locator.Instance.Config.DefaultTimer;
            _timer.Value = duration;
            _targetTimer.State.WaitFor(UIElement.EState.Hided, () => {
                _targetTimer.Load(duration, _timer, _currentTarget.transform);
                _targetTimer.Show();
            });
            _timerTween = DOVirtual
                .Float(duration, 0f, duration, val => _timer.Value = val)
                .SetEase(Ease.Linear)
                .OnComplete(OnTimerEnd);
        }

        private void StopTimer() {
            _targetTimer.Hide();
            _timerTween?.Kill();
        }

        private Pedestrian SpawnPedestrian() {
            var pedestrian = Instantiate(_pedestrianPrefab, _pedestrianParent);

            var position = GetRandomNavMeshPosition();
            var config = Locator.Instance.Config;
            var speed = _rng.NextFloat(config.MinSpeed, config.MaxSpeed);
            pedestrian.Setup(speed, position, GetRandomNavMeshPosition, pos => GetClosestAvailablePosition(pos, true)!.Value);
            pedestrian.OnCollision.Subscribe(OnPedestrianCollision);
            return pedestrian;
        }

        private Vector3 GetRandomNavMeshPosition() {
            var bounds = _mapPlane.bounds;

            while (true) {
                var testX = _rng.NextFloat(bounds.min.x, bounds.max.x);
                var testZ = _rng.NextFloat(bounds.min.z, bounds.max.z);
                var testPosition = new Vector3(testX, bounds.max.y, testZ);

                var position = GetClosestAvailablePosition(testPosition);
                if (position is { } pos) {
                    return pos;
                }
            }
        }

        private Vector3? GetClosestAvailablePosition(Vector3 position, bool retry = false) {
            const float initialMaxDistance = 3f;
            const float maxDistanceModifier = 1.5f;

            var radius = NavMesh.GetSettingsByIndex(0).agentRadius * 1.1f;
            var maxDistance = initialMaxDistance;
            do {
                if (NavMesh.SamplePosition(position, out var hit, maxDistance, NavMesh.AllAreas)
                    && NavMesh.FindClosestEdge(hit.position, out var edgeHit, NavMesh.AllAreas)) {
                    // _edgeNormals.Add((edgeHit.position, edgeHit.normal));
                    // _points.Add((hit.position, hit.normal));

                    return Vector3.Distance(hit.position, edgeHit.position) <= radius
                        ? edgeHit.position + edgeHit.normal * radius
                        : hit.position;
                }

                maxDistance *= maxDistanceModifier;
            } while (retry);

            return null;
        }

        private void Update() {
            if (Input.GetButton("Cancel") && LevelInProgress) {
                OnCancel();
            }
        }

        // private List<(Vector3, Vector3)> _edgeNormals = new List<(Vector3, Vector3)>();
        // private List<(Vector3, Vector3)> _points = new List<(Vector3, Vector3)>();
        //
        // private void OnDrawGizmos() {
        //     Gizmos.color = Color.blue;
        //     foreach (var (pos, norm) in _points) {
        //         Gizmos.DrawSphere(pos, 0.1f);
        //         Gizmos.DrawLine(pos, pos + norm);
        //     }
        //     Gizmos.color = Color.red;
        //     foreach (var (pos, norm) in _edgeNormals) {
        //         Gizmos.DrawSphere(pos, 0.1f);
        //         Gizmos.DrawLine(pos, pos + norm);
        //     }
        // }
    }
}