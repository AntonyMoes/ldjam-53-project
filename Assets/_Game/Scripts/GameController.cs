using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Objects;
using _Game.Scripts.Objects.Pedestrian;
using _Game.Scripts.UI;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.Processes;
using GeneralUtils.UI;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts {
    public class GameController : SingletonBehaviour<GameController> {
        [SerializeField] private TutorialController _tutorialController;
        public TutorialController TutorialController => _tutorialController;
        
        [Header("Objects")]
        [SerializeField] private GameObject _map;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private Pedestrian _pedestrianPrefab;
        [SerializeField] private Transform _pedestrianParent;
        [SerializeField] private MeshCollider _mapPlane;
        [SerializeField] private Goddess _goddess;

        public Player Player { get; private set; }

        private MainMenuWindow _mainMenu;
        private GameUIPanel _gameUIPanel;
        private ExitPanel _exitPanel;

        private float _multiplier;
        private float _currentOrderTime;
        private float _startOrderTime;

        private readonly UpdatedValue<int> _score;
        private readonly UpdatedValue<int> _ordersCompleted;

        private readonly UpdatedValue<float> _patience;
        private List<Pedestrian> _pedestrians;
        private Rng _rng;

        private bool LevelInProgress => _map.activeSelf;
        private readonly UpdatedValue<float> _timer = new UpdatedValue<float>();
        private readonly UpdatedValue<Pedestrian> _currentTarget = new UpdatedValue<Pedestrian>();
        private Tween _timerTween;
        private Tween _timeScaleTween;
        private DitheringController _ditheringController;
        private readonly Dictionary<GameObject, float> _objectsToDelete = new Dictionary<GameObject, float>();
        private bool _lost;
        private bool _firstMisson;

        public GameController() {
            _patience = new UpdatedValue<float>(setter: SetPatience);
            _ordersCompleted = new UpdatedValue<int>();
            _score = new UpdatedValue<int>();
        }

        private static float SetPatience(float value) => Mathf.Clamp(value, 0, Locator.Instance.Config.MaxPatience);

        private void Start() {
            _tutorialController.LoadActions(new Dictionary<TutorialController.TutorialAction, Func<Process>> {
                [TutorialController.TutorialAction.Pause] = () => new AsyncProcess(PauseGame),
                [TutorialController.TutorialAction.Unpause] = () => new AsyncProcess(UnpauseGame),
            });

            var ui = UIController.Instance;
            var slidesPanel = ui.ShowSlidesPanel();
            slidesPanel.State.WaitFor(UIElement.EState.Hided, () => {
                _mainMenu = ui.ShowMainMenuWindow(StartLevelFromMenu);
            });
        }

        public void PauseGame(Action onDone = null) {
            TogglePause(true, onDone);
        }

        public void UnpauseGame(Action onDone = null) {
            TogglePause(false, onDone);
        }

        private void TogglePause(bool pause, Action onDone) {
            _timeScaleTween?.Complete(true);

            const float duration = 0.15f;
            var to = pause ? 0f : 1f;

            _timeScaleTween = DOVirtual.Float(Time.timeScale, to, duration, val => Time.timeScale = val)
                .SetUpdate(true)
                .OnComplete(() => onDone?.Invoke());
        }

        private void StartLevelFromMenu() {
            SoundController.Instance.PlayMusic("SoundTrack");
            StartLevel();
        }

        private void StartLevel() {
            _goddess.SetState(Goddess.State.Idle);
            _lost = false;
            _firstMisson = true;
            _map.SetActive(true);
            _rng = new Rng(Rng.RandomSeed);
            _ordersCompleted.Value = 0;
            _score.Value = 0;
            _timer.Value = 0;

            Player = Instantiate(_playerPrefab, _playerSpawn);
            _cameraController.Target = Player;
            _ditheringController = new DitheringController(Player, _currentTarget, Locator.Instance.MainCamera);

            var config = Locator.Instance.Config;
            _multiplier = config.StartMultiplier;
            _pedestrians = Enumerable.Range(0, config.Population).Select(_ => SpawnPedestrian()).ToList();

            _patience.Value = config.InitialPatience;
            _patience.WaitFor(0f, OnLose);

            _gameUIPanel = UIController.Instance.ShowGameUIPanel(_patience, config.MaxPatience, _ordersCompleted, _score);
            SetTarget();
            if (!SaveManager.GetBool(SaveManager.BoolData.TutorialComplete)) {
                _gameUIPanel.State.WaitFor(UIElement.EState.Shown, () => {
                    _tutorialController.StartTutorial(() => SaveManager.SetBool(SaveManager.BoolData.TutorialComplete, true));
                });
            }
        }

        private void EndLevel(Action onDone) {
            StopTimer();
            if (Player != null) {
                Player.Kill(true);
            }

            var objects = _objectsToDelete.Keys.ToArray();
            _objectsToDelete.Clear();
            foreach (var go in objects) {
                Destroy(go);
            }

            _ditheringController.Dispose();
            _ditheringController = null;

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

        private void UpdateOrdersCompleted() {
            _ordersCompleted.Value += 1;
        }

        private void UpdateScore(int delta) {
            _score.Value += delta;
        }

        private void OnPedestrianCollision(Pedestrian pedestrian) {
            pedestrian.OnCollision.Unsubscribe(OnPedestrianCollision);
            pedestrian.Kill();
            SoundController.Instance.PlaySound("pedestianHit", 0.3f);
            _pedestrians.Remove(pedestrian);
            _pedestrians.Add(SpawnPedestrian());

            var config = Locator.Instance.Config;
            if (pedestrian.IsTarget) {
                _goddess.SetState(Goddess.State.Happy);
                Debug.LogError("Nice!");
                UpdateOrdersCompleted();
                int score = _score.Value;
                UpdateScore((150 - Convert.ToInt32(100 * Mathf.Min((_startOrderTime - _timer.Value) / _currentOrderTime, 1))) * (_patience.Value == 100 ? 2 : 1));
                SoundController.Instance.PlaySound("heroDelivery", 0.3f);
                if (_score.Value / config.DeltaPointsForSpeedUp - score / config.DeltaPointsForSpeedUp > 0) {
                    SpeedUpPedestrian(config.DeltaSpeed);
                }
                Debug.LogError(_ordersCompleted.Value);
                Debug.LogError(_score.Value);
                _patience.Value += config.PatienceOnSuccess;
                SetTarget();
            } else {
                _goddess.SetState(Goddess.State.Angry);
                Debug.LogError("Wrong!");
                SoundController.Instance.PlaySound("sfx_mistake", 1f);
                _patience.Value -= config.PatienceOnMistake;
            }
        }

        private void SpeedUpPedestrian(float deltaSpeed) {
            foreach (var pedestrian in _pedestrians) {
                pedestrian.Speed.Value += deltaSpeed;
            }
        }

        private void OnTimerEnd() {
            Debug.LogError("Late!");
            _goddess.SetState(Goddess.State.Angry);
            SoundController.Instance.PlaySound("sfx_mistake", 1f);
            _patience.Value -= Locator.Instance.Config.PatienceOnFail;

            if (_lost) {
                return;
            }

            _currentTarget.Value .IsTarget = false;
            SetTarget();
        }

        private void OnLose() {
            Debug.LogError("GAME OVER");
            _lost = true;
            StopTimer();
            Player.Kill();
            // Destroy(Player.gameObject);

            _gameUIPanel.Hide();
            _exitPanel = UIController.Instance.ShowExitPanel(EndLevelAndLeave, RestartLevel, _score.Value, _ordersCompleted.Value);
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
            if (_currentTarget.Value != null) {
                // _currentTarget.Value .IsTarget = false;
                _currentTarget.Value  = null;
            }

            if (_pedestrians.Count == 0) {
                return;
            }

            _currentTarget.Value  = _rng.NextChoice(_pedestrians);
            _currentTarget.Value .IsTarget = true;
            SoundController.Instance.PlaySound("newRequest", 0.3f);
            StartTimer();
        }

        private void StartTimer() {
            const float timerDistanceMultiplier = 100f;
            const float multiplierThreshold = 0.9f;
            var config = Locator.Instance.Config;

            var target = _currentTarget.Value;
            var targetPosition = (target.transform.position + target.Destination) / 2f;
            var distance = Logic.Distance(targetPosition, Player.transform.position) / timerDistanceMultiplier;
            var bonus = GetBonusMultiplier();
            _currentOrderTime = config.GuaranteedTimer + _multiplier * config.DefaultTimer * distance + config.BonusTimer * bonus;
            var duration =  _timer.Value + _currentOrderTime;
            if (_firstMisson) {
                duration = Mathf.Max(25f, duration);
                _firstMisson = false;
            }
            _startOrderTime = duration;

            Debug.Log(distance);
            Debug.Log(_currentOrderTime);
            if (_multiplier > multiplierThreshold) {
                _multiplier -= config.DeltaMultiplier;
            }

            _timer.Value = duration;
            _gameUIPanel.TargetTimer.State.WaitFor(UIElement.EState.Hided, () => {
                _gameUIPanel.TargetTimer.Load(duration, _timer, target.transform);
                _gameUIPanel.TargetTimer.Show();
            });
            _timerTween = DOVirtual
                .Float(duration, 0f, duration, val => _timer.Value = val)
                .SetEase(Ease.Linear)
                .OnComplete(OnTimerEnd);
        }

        private void StopTimer() {
            _gameUIPanel.TargetTimer.Hide();
            _timerTween?.Kill();
        }

        private float GetBonusMultiplier() {
            var forward = Player.transform.forward;
            var toTarget = (_currentTarget.Value.transform.position - Player.transform.position).normalized;

            return Vector3.Dot(forward, toTarget) + 1;
        }

        private Pedestrian SpawnPedestrian() {
            var pedestrian = Instantiate(_pedestrianPrefab, _pedestrianParent);

            var position = GetRandomNavMeshPosition();
            var config = Locator.Instance.Config;
            var speed = _rng.NextFloat(config.MinSpeed, config.MaxSpeed) + config.DeltaSpeed * (_score.Value / config.DeltaPointsForSpeedUp);
            pedestrian.Setup(speed, Player.MaxSpeed - 1, position, GetRandomNavMeshPosition, pos => GetClosestAvailablePosition(pos, true)!.Value);
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

        public void ScheduleDeletion(GameObject go, float delay) {
            _objectsToDelete.Add(go, delay);
        }

        private void Update() {
            _ditheringController?.Update();
            var objects = _objectsToDelete.Keys.ToArray();
            foreach (var go in objects) {
                if ((_objectsToDelete[go] -= Time.deltaTime) <= 0) {
                    _objectsToDelete.Remove(go);
                    Destroy(go);
                }
            }
        }
    }
}