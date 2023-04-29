using UnityEngine;

namespace _Game.Scripts {
    public static class Logic {
        public static float Distance(Vector3 from, Vector3 to) {
            var delta = from - to;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.z);
        }
    }
}