using System;
using GeneralUtils;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Objects.Pedestrian {
    public class EvadeState : BasePedestrianState {
        private readonly Func<Vector3, Vector3> _getClosestAvailablePosition;
        private Vector3 _savedPosition;
        private float _lastDistanceToPlayer;

        public EvadeState(NavMeshAgent agent, UpdatedValue<float> speed, Func<Vector3, Vector3> getClosestAvailablePosition) : base(agent, speed) {
            _getClosestAvailablePosition = getClosestAvailablePosition;
        }

        protected override void PerformStateEnter() {
            _savedPosition = Agent.destination;
            _lastDistanceToPlayer = float.MaxValue;
        }

        public override void OnStatePersist(float deltaTime) {
            if (!ShouldEvade()) {
                SwitchState(ShouldBeware() ? PedestrianState.Beware : PedestrianState.Walk);
                return;
            }

            var config = Locator.Instance.Config;
            var playerPosition = GameController.Instance.Player.Transform.position;
            var vectorFromPlayer = Agent.transform.position - playerPosition;
            var distanceToPlayer = vectorFromPlayer.magnitude;
            var lastDistanceToPlayer = _lastDistanceToPlayer;
            _lastDistanceToPlayer = distanceToPlayer;
            if (distanceToPlayer >= lastDistanceToPlayer) {
                return;
            }

            var safePoint = vectorFromPlayer.normalized * config.EvadeRadius + playerPosition;
            Agent.destination = _getClosestAvailablePosition(safePoint);
        }

        public override void OnStateExit() {
            if (Agent.isOnNavMesh)
                Agent.destination = _savedPosition;
        }

        protected override void OnSpeedChanged(float newSpeed) {
            Agent.speed = newSpeed;
        }
    }
}