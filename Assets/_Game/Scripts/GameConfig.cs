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
        [SerializeField] private float _guaranteedTimer;
        public float GuaranteedTimer => _guaranteedTimer;

        [SerializeField] private float _defaultTimer;
        public float DefaultTimer => _defaultTimer;

        [SerializeField] private float _bonusTimer;
        public float BonusTimer => _bonusTimer;

        [SerializeField] private float _startMultiplier;
        public float StartMultiplier => _startMultiplier;

        [SerializeField] private float _deltaMultiplier;
        public float DeltaMultiplier => _deltaMultiplier;

        [Header("City")]
        [SerializeField] private int _population;
        public int Population => _population;

        [Header("Pedestrians")]
        [SerializeField] private float _minSpeed;
        public float MinSpeed => _minSpeed;

        [SerializeField] private float _maxSpeed;
        public float MaxSpeed => _maxSpeed;

        [SerializeField] private float _deltaSpeed;
        public float DeltaSpeed => _deltaSpeed;

        [SerializeField] private int _deltaPointsForSpeedUp;
        public int DeltaPointsForSpeedUp => _deltaPointsForSpeedUp;

        [SerializeField] private float _evadeRadius;
        public float EvadeRadius => _evadeRadius;

        [SerializeField] private float _bewareRadius;
        public float BewareRadius => _bewareRadius;

        [SerializeField] private float _bewareSpeedModifier;
        public float BewareSpeedModifier => _bewareSpeedModifier;
    }
}