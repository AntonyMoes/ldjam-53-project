using System;
using UnityEngine;

namespace _Game.Scripts {
    [Serializable]
    public class GameConfig {
        [Header("Patience")]
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

        [Header("Orders")]
        [SerializeField] private float _defaultTimer;
        public float DefaultTimer => _defaultTimer;
        
        [SerializeField] private float _startMultiplier;
        public float StartMultiplier => _startMultiplier;
        
        [SerializeField] private float _deltaMultiplier;
        public float DeltaMultiplier => _deltaMultiplier;

        [Header("City")]
        [SerializeField] private int _population;
        public int Population => _population;
    }
}