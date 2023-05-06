using System;
using UnityEngine;

namespace _Game.Scripts.Objects.Player {
    public class Wheel : MonoBehaviour {
        [Header("General")]
        [SerializeField] private float _mass;
        
        [Header("Suspention")]
        [SerializeField] private float _springLenght;
        [SerializeField] private float _maxSpringLenght;
        [SerializeField] private float _springDamper;
        [SerializeField] private float _springStrength;

        [Header("Steering")]
        [SerializeField] private float _steeringDrag;
        [SerializeField] private float _wheelRotationSpeed;
        [SerializeField] private float _maxRotation;

        [Header("Acceleration")]
        [SerializeField] private bool _drivingWheel;
        [SerializeField] private float _maxForwardVelocity;
        [SerializeField] private float _maxBackwardVelocity;
        [SerializeField] private float _forwardTorque;
        [SerializeField] private float _backwardTorque;
        [SerializeField] private float _brakeTorque;
        [SerializeField] private float _drag;

        private Vector3 _suspensionForce;
        private Vector3 _steeringForce;
        private Vector3 _accelerationForce;
        private Quaternion _desiredRotation;
        private Vector3 _raycast;
        private Vector3 _hit;

        public void UpdateForces(Rigidbody car, float accelerationInput, float brakeInput, float steeringInput, float deltaTime) {
            var ray = new Ray(transform.position, transform.up * -1);
            _raycast = ray.direction * _maxSpringLenght;
            var hit = Physics.Raycast(ray, out var rayHit, _maxSpringLenght);
            _hit = rayHit.point;
            
            // rotate
            if (_maxRotation > 0f) {
                var localRotation = transform.localRotation.eulerAngles;
                var currentRotation = localRotation.y;
                if (currentRotation > 180f)
                    currentRotation -= 360;
                var desiredRotation = Math.Sign(steeringInput) * _maxRotation; // Math.Sign because it returns zero
                var rotationFactor = (steeringInput == 0 ? -Math.Sign(currentRotation) : steeringInput) * deltaTime; // Math.Sign because it returns zero

                var deltaToDesired = desiredRotation - currentRotation;
                var deltaRotation = rotationFactor * _wheelRotationSpeed;
                var sameDirection = Mathf.Sign(deltaToDesired * deltaRotation) == 1f;
                var rotationAmount = !sameDirection ? 0f : Mathf.Min(1f / Mathf.Abs(deltaRotation / deltaToDesired), 1f); // overshooting protection
                
                if (Mathf.Abs(currentRotation) > 0.001f)
                    Debug.Log($"{transform.GetSiblingIndex()}| Curr: {currentRotation}, Des: {desiredRotation}, Unclamped: {deltaRotation}, Amnt: {rotationAmount}, Clamped: {deltaRotation * rotationAmount}");
                transform.Rotate(Vector3.up, deltaRotation * rotationAmount);
                _desiredRotation = Quaternion.Euler(0, desiredRotation, 0);
            }

            if (hit) {
                var worldVelocity = car.GetPointVelocity(transform.position);

                // suspension
                var suspensionDirection = transform.up;
                var springOffset = _springLenght - rayHit.distance;
                var springVelocity = Vector3.Dot(suspensionDirection, worldVelocity);
                var suspensionForce = springOffset * _springStrength - springVelocity * _springDamper;
                car.AddForceAtPosition(suspensionDirection * suspensionForce, transform.position);
                _suspensionForce = suspensionDirection * suspensionForce;

                // steering
                var steeringDragDirection = transform.right;
                var steeringVelocity = Vector3.Dot(steeringDragDirection, worldVelocity);
                var desiredSteeringChange = -steeringVelocity * _steeringDrag;
                var desiredSteeringForce = _mass * desiredSteeringChange / deltaTime;
                car.AddForceAtPosition(steeringDragDirection * desiredSteeringForce, transform.position);
                _steeringForce = steeringDragDirection * desiredSteeringForce;

                // acceleration
                var accelerationDirection = transform.forward;
                var forwardVelocity = Vector3.Dot(car.transform.forward, car.velocity);

                //// torque
                var forward = Mathf.Sign(accelerationInput) >= 0;
                var maxVelocity = forward ? _maxForwardVelocity : _maxBackwardVelocity;
                var normalizedVelocity = Mathf.Clamp01(Mathf.Abs(forwardVelocity) / maxVelocity);
                var torque = forward ? _forwardTorque : _backwardTorque;
                var clampedTorque = normalizedVelocity == 1f || !_drivingWheel ? 0f : torque;

                //// drag
                var desiredVelocityChange = -forwardVelocity * _drag;
                var desiredDragForce = _mass * desiredVelocityChange / deltaTime;

                var accelerationForce = (accelerationInput, brakeInput) switch {
                    (0f, 0f) => desiredDragForce,  // dragging
                    (0f, _) => _brakeTorque * brakeInput,  // braking
                    (_, 0f) => clampedTorque * accelerationInput,  // accelerating
                    _ => throw new ArgumentOutOfRangeException()
                };
                car.AddForceAtPosition(accelerationDirection * accelerationForce, transform.position);
                _accelerationForce = accelerationDirection * accelerationForce;
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_hit, 0.05f);
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawLine(transform.position, transform.position + _raycast);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _suspensionForce);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _steeringForce);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _accelerationForce);

            Gizmos.color = Color.magenta;
            var delta = _desiredRotation * Quaternion.Inverse(transform.localRotation);
            Gizmos.DrawLine(transform.position, transform.position + delta * transform.forward);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        }
    }
}