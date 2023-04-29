using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Objects;
using _Game.Scripts.UI.Base;
using GeneralUtils;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts {
    public class GameController : SingletonBehaviour<GameController> {
        [Header("Game UI")]
        [SerializeField] private ProgressBar _patienceProgressBar;

        [Header("Objects")]
        [SerializeField] private Player _player;
        [SerializeField] private Pedestrian _pedestrianPrefab;
        [SerializeField] private Transform _pedestrianParent;
        [SerializeField] private MeshCollider _mapPlane;

        [Header("Config")]
        [SerializeField] private GameConfig _config;

        private readonly UpdatedValue<float> _patience;
        private List<Pedestrian> _pedestrians;
        private Rng _rng;

        public GameController() {
            _patience = new UpdatedValue<float>(setter: SetPatience);
        }

        private float SetPatience(float value) => Mathf.Clamp(value, 0, _config.MaxPatience);

        private void Start() {
            _rng = new Rng(Rng.RandomSeed);

            // _pedestrians = FindObjectsOfType<Pedestrian>().ToList();
            _pedestrians = Enumerable.Range(0, _config.Population).Select(_ => SpawnPedestrian()).ToList();
            SetTarget();

            _patience.Value = _config.InitialPatience;
            _patienceProgressBar.Load(0, _config.MaxPatience);
            _patience.Subscribe(val => _patienceProgressBar.CurrentValue = val, true);
        }

        private void OnPedestrianCollision(Pedestrian pedestrian) {
            pedestrian.OnCollision.Unsubscribe(OnPedestrianCollision);
            pedestrian.Kill();
            _pedestrians.Remove(pedestrian);
            _pedestrians.Add(SpawnPedestrian());

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
        }

        private void SetTarget() {
            if (_pedestrians.Count == 0) {
                return;
            }

            _rng.NextChoice(_pedestrians).IsTarget = true;
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