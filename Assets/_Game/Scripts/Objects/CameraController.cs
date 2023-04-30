using UnityEngine;

namespace _Game.Scripts.Objects {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Vector3 _delta;
        [SerializeField] private float _smoothSpeed;

        public Player Target { get; set; }

        private void FixedUpdate() {
            if (Target == null) {
                return;
            }

            // transform.position = Vector3.Lerp(transform.position, Target.LookPoint.position + _delta, _smoothSpeed);
            transform.position = Vector3.Lerp(transform.position, Target.transform.position + _delta, _smoothSpeed);
            // transform.LookAt(Target.LookPoint);
            transform.LookAt(Target.transform);
            // transform.position = Target.transform.position + _delta;
        }
    }
}
