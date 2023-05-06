using UnityEngine;

namespace _Game.Scripts.Objects.Player {
    public abstract class Player : MonoBehaviour {
        [SerializeField] private Transform[] _ditherCheckPoints;
        public Transform[] DitherCheckPoints => _ditherCheckPoints;

        public Transform Transform => transform;

        public abstract float MaxSpeed { get; }
        public abstract void Kill(bool immediate = false);
    }
}