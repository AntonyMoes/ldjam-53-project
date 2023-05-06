using GeneralUtils;
using GeneralUtils.States;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Objects.Pedestrian {
    public abstract class BasePedestrianState : State<PedestrianState> {
        protected readonly NavMeshAgent Agent;
        protected readonly UpdatedValue<float> Speed;

        protected BasePedestrianState(NavMeshAgent agent, UpdatedValue<float>  speed) {
            Agent = agent;
            Speed = speed;
            Speed.Subscribe(OnSpeedChanged);
        }

        protected virtual void OnSpeedChanged(float newSpeed) { }

        protected bool ShouldEvade() {
            if (GameController.Instance.Player == null) {
                return false;
            }

            var config = Locator.Instance.Config;
            var distanceToPlayer = Vector3.Distance(GameController.Instance.Player.Transform.position, Agent.transform.position);
            return distanceToPlayer <= config.EvadeRadius;
        }

        protected bool ShouldBeware() {
            if (GameController.Instance.Player == null) {
                return false;
            }

            var config = Locator.Instance.Config;
            var distanceToPlayer = Vector3.Distance(GameController.Instance.Player.Transform.position, Agent.transform.position);
            return distanceToPlayer <= config.BewareRadius;
        }

        protected bool Arrived() {
            return !Agent.pathPending
                   && Agent.remainingDistance <= Agent.stoppingDistance
                   && (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f);
        }
    }
}