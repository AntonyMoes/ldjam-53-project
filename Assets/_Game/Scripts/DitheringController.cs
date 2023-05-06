using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Objects;
using _Game.Scripts.Objects.Pedestrian;
using _Game.Scripts.Objects.Player;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class DitheringController {
        private readonly Player _player;
        private readonly UpdatedValue<Pedestrian> _target;
        private readonly Camera _checkCamera;

        private readonly List<Building> _ditheredBuildings = new List<Building>();
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[5];


        public DitheringController(Player player, UpdatedValue<Pedestrian> target, Camera checkCamera) {
            _player = player;
            _target = target;
            _checkCamera = checkCamera;
        }

        public void Update() {
            if (_player == null) {
                return;
            }

            var newDitheredBuildings =
                (_target.Value == null ? _player.DitherCheckPoints : _player.DitherCheckPoints.Append(_target.Value.transform))
                .Select(CheckPoint)
                .SelectMany(x => x)
                .ToHashSet();

            var stopDithering = _ditheredBuildings.Where(building => !newDitheredBuildings.Contains(building)).ToArray();
            var startDithering = newDitheredBuildings.Where(building => !_ditheredBuildings.Contains(building)).ToArray();

            foreach (var building in stopDithering) {
                building.ToggleDithering(false);
                _ditheredBuildings.Remove(building);
            }

            foreach (var building in startDithering) {
                building.ToggleDithering(true);
                _ditheredBuildings.Add(building);
            }
        }

        private Building[] CheckPoint(Transform point) {
            var result = new List<Building>();

            var checkOrigin = _checkCamera.transform.position;
            var checkVector = point.position - checkOrigin;
            var hitCount = Physics.RaycastNonAlloc(checkOrigin, checkVector.normalized, _hitBuffer, checkVector.magnitude);
            for (var i = 0; i < hitCount; i++) {
                var hit = _hitBuffer[i];
                var building = hit.collider.GetComponentInParent<Building>();
                if (building != null) {
                    result.Add(building);
                }
            }

            return result.ToArray();
        }

        public void Dispose() {
            foreach (var building in _ditheredBuildings) {
                building.ResetDithering();
            }
        }
    }
}