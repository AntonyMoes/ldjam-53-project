using UnityEngine;

namespace _Game.Scripts.Objects {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Vector3 _delta;
        [SerializeField] private float _smoothSpeed;

        private Player.Player _player;

        public Player.Player Target {
            get => _player;
            set {
                _player = value;
                UpdatePosition(true);
            }
        }

        private void FixedUpdate() {
            if (Target == null) {
                return;
            }

            // transform.position = Vector3.Lerp(transform.position, Target.LookPoint.position + _delta, _smoothSpeed);
            // transform.LookAt(Target.LookPoint);
            // transform.position = Target.transform.position + _delta;
            UpdatePosition();
        }

        private void UpdatePosition(bool force = false) {
            if (Target == null) {
                return;
            }

            var targetPosition = Target.Transform.position + _delta;
            transform.position = force ? targetPosition : Vector3.Lerp(transform.position, targetPosition, _smoothSpeed);
            transform.LookAt(Target.Transform);
        }
    }
}
