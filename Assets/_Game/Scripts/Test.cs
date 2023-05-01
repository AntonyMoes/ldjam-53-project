using _Game.Scripts.UI;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class Test : MonoBehaviour {
        [SerializeField] private Transform _target;
        [SerializeField] private TargetTimer _targetTimer;

        private void Start() {
            var a = new UpdatedValue<float>(66);
            _targetTimer.Load(66, a, _target);
            _targetTimer.Show();
        }
    }
}