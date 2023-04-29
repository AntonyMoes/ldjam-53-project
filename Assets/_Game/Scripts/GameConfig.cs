using System;
using UnityEngine;

namespace _Game.Scripts {
    [Serializable]
    public class GameConfig {
        [SerializeField] private float _maxPatience;
        public float MaxPatience => _maxPatience;

        [SerializeField] private float _initialPatience;
        public float InitialPatience => _initialPatience;

        [SerializeField] private float _patienceOnSuccess;
        public float PatienceOnSuccess => _patienceOnSuccess;

        [SerializeField] private float _patienceOnFail;
        public float PatienceOnFail => _patienceOnFail;

        [SerializeField] private float _patienceOnMistake;
        public float PatienceOnMistake => _patienceOnMistake;

        [SerializeField] private int _population;
        public int Population => _population;
    }
}