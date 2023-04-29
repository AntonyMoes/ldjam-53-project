using UnityEngine;

namespace _Game.Scripts {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _delta;

        private void LateUpdate() {
            transform.position = _target.position + _delta;
        }
    }
}
