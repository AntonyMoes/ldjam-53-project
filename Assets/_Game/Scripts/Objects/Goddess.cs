using UnityEngine;

namespace _Game.Scripts.Objects {
    public class Goddess : MonoBehaviour {
        [SerializeField] private Animator _animator;

        private State _state = State.Idle;

        public void SetState(State state) {
            if (state == _state) {
                return;
            }

            _animator.SetTrigger(state.ToString().ToLower());
        }

        private void OnEnter(State state) {
            _state = state;
        }

        public enum State {
            Idle,
            Angry,
            Happy
        }
    }
}