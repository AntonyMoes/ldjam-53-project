using System;
using DG.Tweening;
using GeneralUtils;
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

        public Vector3 Destination => _agent.destination;

        private bool _isTarget;
        public bool IsTarget {
            get => _isTarget;
            set {
                _isTarget = value;
                _renderer.material = value ? _targetMaterial : _defaultMaterial;
            }
        }

        public readonly UpdatedValue<float> Speed = new UpdatedValue<float>();

        private readonly StateMachine<PedestrianState> _stateMachine = new StateMachine<PedestrianState>();
        private bool _killed;
        private Tween _destructionAnimation;

        public Pedestrian() {
            OnCollision = new Event<Pedestrian>(out _onCollision);
        }

        private void OnCollisionEnter(Collision collision) {
            if (!_collisionMask.Includes(collision.gameObject.layer)) {
                return;
            }

            _onCollision(this);
            Physics.IgnoreCollision(collision.collider, _collider);
        }

        public void Setup(float speed, Vector3 startPosition, Func<Vector3> getNextPosition, Func<Vector3, Vector3> getClosestAvailablePosition) {
            Speed.Value = speed;
            transform.position = startPosition;
            _rb.useGravity = false;

            _stateMachine.AddState(PedestrianState.Walk, new WalkState(_agent, Speed, getNextPosition));
            _stateMachine.AddState(PedestrianState.Beware, new BewareState(_agent, Speed, getNextPosition));
            _stateMachine.AddState(PedestrianState.Evade, new EvadeState(_agent, Speed, getClosestAvailablePosition));
            _stateMachine.SetDefaultState(PedestrianState.Walk);
            _stateMachine.Start();
        }

        public void Kill(bool immediate = false) {
            _stateMachine.Stop();
            _killed = true;
            _agent.enabled = false;
            _rb.useGravity = true;

            if (immediate) {
                _destructionAnimation?.Kill();
                Destroy(gameObject);
            } else {
                // TODO
                _destructionAnimation = DOVirtual.DelayedCall(1f, () => Destroy(gameObject));
            }
        }

        private void Update() {
            if (_killed) {
                return;
            }

            _stateMachine.Update(Time.deltaTime);
        }
    }
}