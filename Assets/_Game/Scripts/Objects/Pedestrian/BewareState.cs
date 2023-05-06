using System;
using GeneralUtils;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Objects.Pedestrian {
    public class BewareState : BasePedestrianState {
        private readonly Func<Vector3> _getNextPosition;

        public BewareState(NavMeshAgent agent, UpdatedValue<float> speed, Func<Vector3> getNextPosition) : base(agent, speed) {
            _getNextPosition = getNextPosition;
        }

        public override void OnStatePersist(float deltaTime) {
            if (!ShouldBeware()) {
                SwitchState(PedestrianState.Walk);
                return;
            }

            if (ShouldEvade()) {
                SwitchState(PedestrianState.Evade);
                return;
            }

            if (Arrived()) {
                Agent.SetDestination(_getNextPosition());
            }

            Agent.speed = GoingInPlayerDirection()
                ? Speed.Value * Locator.Instance.Config.BewareSpeedModifier
                : Speed.Value;
        }

        public override void OnStateExit() {
            Agent.speed = Speed.Value;
        }

        private bool GoingInPlayerDirection() {
            var vectorToPlayer = GameController.Instance.Player.Transform.position - Agent.transform.position;

            return Vector3.Angle(Agent.velocity, vectorToPlayer) <= 90;
        }
    }
}