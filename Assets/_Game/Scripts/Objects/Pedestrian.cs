using System;
using UnityEngine;

namespace _Game.Scripts.Objects {
    public class Pedestrian : MonoBehaviour {
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _targetMaterial;
        [SerializeField] private Renderer _renderer;

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

        public Pedestrian() {
            OnCollision = new GeneralUtils.Event<Pedestrian>(out _onCollision);
        }

        private void OnCollisionEnter(Collision collision) {
            if (!_collisionMask.Includes(collision.gameObject.layer)) {
                return;
            }

            _onCollision(this);
        }
    }
}