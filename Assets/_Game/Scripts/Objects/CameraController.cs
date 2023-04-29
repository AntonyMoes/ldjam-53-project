using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts.Objects {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Vector3 _delta;

        public Transform Target { get; set; }

        private void LateUpdate() {
            if (Target == null) {
                return;
            }

            transform.position = Target.position + _delta;
        }
    }
}
