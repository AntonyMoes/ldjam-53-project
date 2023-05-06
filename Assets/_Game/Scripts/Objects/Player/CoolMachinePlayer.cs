using UnityEngine;

namespace _Game.Scripts.Objects.Player {
    public class CoolMachinePlayer : Player {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Wheel[] _wheels;

        public override float MaxSpeed => 0f;

        private float CurrentVelocity => Vector3.Dot(_rb.velocity, _rb.transform.forward);
        private bool ZeroVelocity => Mathf.Abs(CurrentVelocity) < 0.0001f; 

        private void FixedUpdate() {
            var vertical = Input.GetAxisRaw("Vertical");
            var horizontal = Input.GetAxisRaw("Horizontal");

            var acceleration = ZeroVelocity
                ? vertical
                : CurrentVelocity > 0
                    ? Mathf.Clamp(vertical, 0, 1)
                    : Mathf.Clamp(vertical, -1, 0);
            var brake = ZeroVelocity
                ? 0
                : CurrentVelocity > 0
                    ? Mathf.Clamp(vertical, -1, 0)
                    : Mathf.Clamp(vertical, 0, 1);

            foreach (var wheel in _wheels) {
                wheel.UpdateForces(_rb, acceleration, brake, horizontal, Time.fixedDeltaTime);
            }
        }

        public override void Kill(bool immediate = false) {
            if (gameObject != null) {
                Destroy(gameObject);
            }
        }
    }
}
