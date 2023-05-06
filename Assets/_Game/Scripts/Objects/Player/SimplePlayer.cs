using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GeneralUtils;
using UnityEngine;
using UnityEngine.VFX;

namespace _Game.Scripts.Objects.Player {
    public class SimplePlayer : Player {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Transform _lookPoint;
        [SerializeField] private Transform _vfx;
        [SerializeField] private VisualEffect _smoke;

        [Header("Parameters")]
        [SerializeField] private float _acceleration;
        [SerializeField] private float _backAcceleration;
        [SerializeField] private float _brakeAcceleration;
        [SerializeField] private float _deceleration;
        [SerializeField] private float _minSteerRadius;
        [SerializeField] private float _maxSteerRadius;
        [SerializeField] private float _maxVelocity;
        [SerializeField] private float _maxBackVelocity;

        public override float MaxSpeed => _maxVelocity;

        private float CurrentVelocity => Vector3.Dot(_rb.velocity, _rb.transform.forward);
        private readonly Dictionary<string, AudioSource> _sounds = new Dictionary<string, AudioSource>();
        private float _lastVertical = 0;
        private Tween _destructionAnimation;
        private bool _killed;
        private float _initialY;

        private void Start() {
            _initialY = transform.position.y;
        }

        private void TurnSoundOn(string soundName, float volume = 0.3f, bool loop = false, bool reset = false) {
            if (_sounds.TryGetValue(soundName, out var source) && !source.isPlaying) {
                _sounds.Remove(soundName);
            }

            if (_sounds.ContainsKey(soundName) && reset) {
                TurnSoundOff(soundName);
            }

            if (!_sounds.ContainsKey(soundName)) {
                _sounds[soundName] = SoundController.Instance.PlaySound(soundName, volume);
            }
            _sounds[soundName].loop = loop;
        }

        private void TurnSoundOff(string soundName) {
            if (_sounds.ContainsKey(soundName)) {
                var source = _sounds[soundName];
                if (source == null) {
                    return;
                }

                source.DOFade(0f, .15f).OnComplete(() => {
                    source.Stop();
                });
                _sounds.Remove(soundName);
            }
        }

        private void FixedUpdate() {
            if (_killed) {
                return;
            }

            var vertical = Input.GetAxisRaw("Vertical");
            var horizontal = Input.GetAxisRaw("Horizontal");

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

            if (CurrentVelocity == 0) {
                TurnSoundOn("stayStill", 0.3f, true);
            } else {
                TurnSoundOff("stayStill");
            }

            if (CurrentVelocity == 0 && vertical != 0) {
                TurnSoundOn("gearShift", 0.6f, false);
            } else {
                TurnSoundOff("gearShift");
            }

            if (vertical != 0 && _lastVertical == 0) {
                TurnSoundOn("pressW", 0.4f, reset: true);
            }

            if (vertical != 0 || CurrentVelocity != 0) {
                TurnSoundOn("holdingW", 5.6f, true);
            } else {
                TurnSoundOff("holdingW");
            }

            if (Mathf.Abs(brake) > 0) {
                TurnSoundOn("brake2", 0.6f, false);
            } else {
                TurnSoundOff("brake2");
            }

            UpdateVelocity(acceleration, brake, horizontal, Time.fixedDeltaTime);
            _lastVertical = vertical;

            var directionMultiplier = CurrentVelocity < 0f ? 0.3f : 1f;
            var power = Mathf.Clamp(Mathf.Abs(CurrentVelocity) / _maxVelocity * directionMultiplier, 0.05f, 1f);
            _smoke.SetFloat("POWER", power);
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
                var radius = Mathf.Abs(CurrentVelocity) / _maxVelocity * (_maxSteerRadius - _minSteerRadius) + _minSteerRadius;
                var circumference = radius * 2 * Mathf.PI;
                var angle = CurrentVelocity * deltaTime / circumference * 360;
                var adjustedAngle = angle * Mathf.Sign(steering);
                _rb.rotation *= Quaternion.Euler(0, adjustedAngle, 0);
            }

            var delta = (acceleration + brake) * deltaTime;
            if (delta == 0) {
                delta = -Mathf.Sign(CurrentVelocity) * Mathf.Min(Mathf.Abs(CurrentVelocity), _deceleration * deltaTime);
            }

            _rb.velocity = transform.forward * Mathf.Clamp(CurrentVelocity + delta, -_maxBackVelocity, _maxVelocity);
            transform.position = transform.position.With(y: _initialY);
        }

        public override void Kill(bool immediate = false) {
            _killed = true;

            if (immediate) {
                _destructionAnimation?.Kill();
                Destroy(gameObject);
                foreach (var value in _sounds.Keys.ToArray()) {
                    TurnSoundOff(value);
                }
            } else {
                _destructionAnimation =
                    DOTween.Sequence()
                        .Append(DOVirtual.Float(HorizontalVelocity().magnitude, 0, 0.3f, val => {
                            var vel = HorizontalVelocity().normalized * val;
                            _rb.velocity = new Vector3(vel.x, _rb.velocity.y, vel.y);
                        }).SetEase(Ease.OutSine))
                        .AppendCallback(() => {
                            var vfx = Instantiate(_vfx);
                            vfx.position = transform.position;
                            GameController.Instance.ScheduleDeletion(vfx.gameObject, 1f);
                            SoundController.Instance.PlaySound("thunder", 0.25f);
                        })
                        .AppendInterval(0.15f)
                        .AppendCallback(() => Destroy(gameObject));
            }

            Vector2 HorizontalVelocity() {
                return new Vector2(_rb.velocity.x, _rb.velocity.z);
            }
        }

        private void OnCollisionEnter(Collision collision) {
            if (!collision.gameObject.TryGetComponent(out Pedestrian.Pedestrian _)) {
                TurnSoundOn("bumpWall", 0.8f);
            }
        }
    }
}
