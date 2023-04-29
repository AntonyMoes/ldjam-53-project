using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Objects;
using _Game.Scripts.UI;
using _Game.Scripts.UI.Base;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.UI;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts {
    public class GameController : SingletonBehaviour<GameController> {
        [Header("UI")]
        [SerializeField] private ProgressBar _patienceProgressBar;
        [SerializeField] private TargetTimer _targetTimer;

        [Header("Objects")]
        [SerializeField] private Player _player;
        [SerializeField] private Pedestrian _pedestrianPrefab;
        [SerializeField] private Transform _pedestrianParent;
        [SerializeField] private MeshCollider _mapPlane;

        [Header("Config")]
        [SerializeField] private GameConfig _config;

        [Header("TimerFormula")]
        private float _multiplier;

        private readonly UpdatedValue<float> _patience;
        private List<Pedestrian> _pedestrians;
        private Rng _rng;

        private readonly UpdatedValue<float> _timer = new UpdatedValue<float>();
        private Pedestrian _currentTarget;
        private Tween _timerTween;
        private bool _lost;

        public GameController() {
            _patience = new UpdatedValue<float>(setter: SetPatience);
        }

        private float SetPatience(float value) => Mathf.Clamp(value, 0, _config.MaxPatience);

        private void Start() {
            _rng = new Rng(Rng.RandomSeed);
            _multiplier = _config.StartMultiplier;

            // _pedestrians = FindObjectsOfType<Pedestrian>().ToList();
            _pedestrians = Enumerable.Range(0, _config.Population).Select(_ => SpawnPedestrian()).ToList();
            SetTarget();

            _patience.Value = _config.InitialPatience;
            _patienceProgressBar.Load(0, _config.MaxPatience);
            _patience.Subscribe(val => _patienceProgressBar.CurrentValue = val, true);
            _patience.WaitFor(0f, OnLose);
        }

        private void OnPedestrianCollision(Pedestrian pedestrian) {
            pedestrian.OnCollision.Unsubscribe(OnPedestrianCollision);
            pedestrian.Kill();
            _pedestrians.Remove(pedestrian);
            _pedestrians.Add(SpawnPedestrian());

            // TODO
            if (pedestrian.IsTarget) {
                Debug.LogError("Nice!");
                _patience.Value += _config.PatienceOnSuccess;
                SetTarget();
            } else {
                Debug.LogError("Wrong!");
                _patience.Value -= _config.PatienceOnMistake;
            }
        }

        private void OnTimerEnd() {
            Debug.LogError("Late!");
            _patience.Value -= _config.PatienceOnFail;

            if (_lost) {
                return;
            }

            SetTarget();
        }

        private void OnLose() {
            Debug.LogError("GAME OVER");
            _lost = true;
            StopTimer();
            Destroy(_player.gameObject);
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
            var duration = _timer.Value + _config.DefaultTimer * _multiplier;
            if (_multiplier > 1)
                _multiplier -= _config.DeltaMultiplier;

            _timer.Value += duration;
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
            _targetTimer.Unload();
            _timerTween?.Kill();
        }

        private Pedestrian SpawnPedestrian() {
            var position = GetRandomNavMeshPosition();
            var pedestrian = Instantiate(_pedestrianPrefab, _pedestrianParent);
            pedestrian.Setup(position, GetRandomNavMeshPosition);
            pedestrian.OnCollision.Subscribe(OnPedestrianCollision);
            return pedestrian;
        }

        private Vector3 GetRandomNavMeshPosition() {
            const float maxDistance = 3f;
            var bounds = _mapPlane.bounds;
            var radius = NavMesh.GetSettingsByIndex(0).agentRadius * 1.1f;

            while (true) {
                var testX = _rng.NextFloat(bounds.min.x, bounds.max.x);
                var testZ = _rng.NextFloat(bounds.min.z, bounds.max.z);
                var testPosition = new Vector3(testX, bounds.max.y, testZ);

                if (NavMesh.SamplePosition(testPosition, out var hit, maxDistance, NavMesh.AllAreas)
                    && NavMesh.FindClosestEdge(hit.position, out var edgeHit, NavMesh.AllAreas)) {
                    // _edgeNormals.Add((edgeHit.position, edgeHit.normal));
                    // _points.Add((hit.position, hit.normal));

                    return Vector3.Distance(hit.position, edgeHit.position) <= radius
                        ? edgeHit.position + edgeHit.normal * radius
                        : hit.position;
                }
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