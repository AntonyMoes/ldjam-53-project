using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Objects {
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

        private bool _killed;
        private Func<Vector3> _getNextPosition;

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

        public void Setup(Vector3 startPosition, Func<Vector3> getNextPosition) {
            transform.position = startPosition;
            _getNextPosition = getNextPosition;
            _agent.SetDestination(_getNextPosition());
            _rb.useGravity = false;
        }

        public void Kill() {
            _killed = true;
            _agent.enabled = false;
            _rb.useGravity = true;
            DOVirtual.DelayedCall(1f, () => Destroy(gameObject));
        }

        private void Update() {
            if (_killed) {
                return;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance && (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)) {
                _agent.SetDestination(_getNextPosition());
            }
        }
    }
}