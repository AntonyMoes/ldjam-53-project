using System;
using DG.Tweening;
using GeneralUtils.States;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Objects.Pedestrian {
    public class Pedestrian : MonoBehaviour {
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _targetMaterial;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Collider _collider;
        [SerializeField] private Rigidbody _rb;

        private readonly Action<Pedestrian> _onCollision;
        public GeneralUtils.Event<Pedestrian> OnCollision { get; }

        private bool _isTarget;
        public bool IsTarget {
            get => _isTarget;
            set {
                _isTarget = value;
                _renderer.material = value ? _targetMaterial : _defaultMaterial;
            }
        }

        private readonly StateMachine<PedestrianState> _stateMachine = new StateMachine<PedestrianState>();
        // private float _speed;
        // private Func<Vector3> _getNextPosition;
        // private Func<Vector3, Vector3> _getClosestAvailablePosition;
        private bool _killed;

        public Pedestrian() {
            OnCollision = new GeneralUtils.Event<Pedestrian>(out _onCollision);
        }

        private void OnCollisionEnter(Collision collision) {
            if (!_collisionMask.Includes(collision.gameObject.layer)) {
                return;
            }

            _onCollision(this);
            Physics.IgnoreCollision(collision.collider, _collider);
        }

        public void Setup(float speed, Vector3 startPosition, Func<Vector3> getNextPosition, Func<Vector3, Vector3> getClosestAvailablePosition) {
            // _speed = speed;
            // _agent.speed = speed;
            transform.position = startPosition;
            // _getNextPosition = getNextPosition;
            // _getClosestAvailablePosition = getClosestAvailablePosition;
            // _agent.SetDestination(_getNextPosition());
            _rb.useGravity = false;

            _stateMachine.AddState(PedestrianState.Walk, new WalkState(_agent, speed, getNextPosition));
            _stateMachine.AddState(PedestrianState.Beware, new BewareState(_agent, speed, getNextPosition));
            _stateMachine.AddState(PedestrianState.Evade, new EvadeState(_agent, speed, getClosestAvailablePosition));
            _stateMachine.SetDefaultState(PedestrianState.Walk);
            _stateMachine.Start();
        }

        public void Kill() {
            _stateMachine.Stop();
            _killed = true;
            _agent.enabled = false;
            _rb.useGravity = true;
            DOVirtual.DelayedCall(1f, () => Destroy(gameObject));
        }

        private void Update() {
            if (_killed) {
                return;
            }

            _stateMachine.Update(Time.deltaTime);
            // var config = Locator.Instance.Config;
            // var distanceToPlayer = Vector3.Distance(GameController.Instance.Player.transform.position, transform.position);
            // if (distanceToPlayer < config.EvadeRadius) {
            //     // go to state: evade
            // }
            //
            // if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance && (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)) {
            //     // state: came to destination
            //     _agent.SetDestination(_getNextPosition());
            // }
            //
            // // state: moving
        }
    }
}