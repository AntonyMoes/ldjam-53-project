using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts.Objects {
    public class Player : MonoBehaviour {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Transform[] _ditherCheckPoints;
        public Transform[] DitherCheckPoints => _ditherCheckPoints;

        [Header("Parameters")]
        [SerializeField] private float _acceleration;
        [SerializeField] private float _backAcceleration;
        [SerializeField] private float _brakeAcceleration;
        [SerializeField] private float _deceleration;
        [SerializeField] private float _minSteerRadius;
        [SerializeField] private float _maxSteerRadius;
        [SerializeField] private float _maxVelocity;
        [SerializeField] private float _maxBackVelocity;

        private float CurrentVelocity => Vector3.Dot(_rb.velocity, _rb.transform.forward);

        private void FixedUpdate() {
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");

            var acceleration = CurrentVelocity == 0
                ? vertical
                : CurrentVelocity > 0
                    ? Mathf.Clamp(vertical, 0, 1) * _acceleration
                    : Mathf.Clamp(vertical, -1, 0) * _backAcceleration;
            var brake = (CurrentVelocity == 0
                ? 0
                : CurrentVelocity > 0
                    ? Mathf.Clamp(vertical, -1, 0)
                    : Mathf.Clamp(vertical, 0, 1))
                        * _brakeAcceleration;
            
            UpdateVelocity(acceleration, brake, horizontal, Time.fixedDeltaTime);
        }

        private void OnDrawGizmos() {
            // // Debug.Log($"Vel: {_rb.velocity}, For: {_rb.transform.forward}, Proj: {Vector3.Project(_rb.velocity, _rb.transform.forward)}");
            // var vertical = Input.GetAxisRaw("Vertical");
            // var horizontal = Input.GetAxisRaw("Horizontal");
            //
            // var acceleration = CurrentVelocity == 0
            //     ? vertical
            //     : CurrentVelocity > 0
            //         ? Mathf.Clamp(vertical, 0, 1) * _acceleration
            //         : Mathf.Clamp(vertical, -1, 0) * _backAcceleration;
            // var brake = (CurrentVelocity == 0
            //                 ? 0
            //                 : CurrentVelocity > 0
            //                     ? -1 * Mathf.Clamp(vertical, -1, 0)
            //                     : Mathf.Clamp(vertical, 0, 1))
            //             * _brakeAcceleration;
            //
            // var center = transform.position;
            // var forward = transform.forward;
            // var right = transform.right;
            // Gizmos.color = Color.blue;
            // Gizmos.DrawLine(center, center + forward * acceleration * 3);
            // Gizmos.color = Color.red;
            // Gizmos.DrawLine(center, center + forward * brake  * 3);
            // // Gizmos.DrawLine(center, center + _rb.transform.forward  * 3);
            // // Gizmos.color = Color.cyan;
            // // Gizmos.DrawLine(center, center + right * horizontal * 3);
            // Gizmos.color = Color.black;
            // Gizmos.DrawLine(center, center + _rb.velocity * 5);
            
            Gizmos.color = Color.blue.WithAlpha(0.1f);
            Gizmos.DrawSphere(transform.position, Locator.Instance.Config.BewareRadius);
            Gizmos.color = Color.red.WithAlpha(0.1f);;
            Gizmos.DrawSphere(transform.position, Locator.Instance.Config.EvadeRadius);
        }

        private void UpdateVelocity(float acceleration, float brake, float steering, float deltaTime) {
            if (CurrentVelocity != 0 && steering != 0) {
                // Debug.Log($"Vel: {CurrentVelocity}, Steer: {steering}");
                var radius = Mathf.Abs(CurrentVelocity) / _maxVelocity * (_maxSteerRadius - _minSteerRadius) + _minSteerRadius;
                var circumference = radius * 2 * Mathf.PI;
                var angles = CurrentVelocity * deltaTime / circumference * 360;
                // Debug.Log($"Rad: {radius}, Ang: {angles}");
                _rb.rotation *= Quaternion.Euler(0, angles * Mathf.Sign(steering), 0);
                // _rb.rotation 
            }

            // Debug.Log($"Acc: {acceleration}, Br: {brake}");
            var delta = (acceleration + brake) * deltaTime;
            if (delta == 0) {
                delta = -Mathf.Sign(CurrentVelocity) * Mathf.Min(Mathf.Abs(CurrentVelocity), _deceleration * deltaTime);
            }

            _rb.velocity = transform.forward * Mathf.Clamp(CurrentVelocity + delta, -_maxBackVelocity, _maxVelocity);
        }
    }
}
