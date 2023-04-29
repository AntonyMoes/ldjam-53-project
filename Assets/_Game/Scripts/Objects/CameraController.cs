using UnityEngine;

namespace _Game.Scripts.Objects {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _delta;

        private void LateUpdate() {
            if (_target == null) {
                return;
            }

            transform.position = _target.position + _delta;
        }
    }
}
