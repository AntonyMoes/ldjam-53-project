using UnityEngine;

namespace _Game.Scripts {
    public static class Extensions {
        public static bool Includes(this LayerMask mask, int layer) => mask == (mask | (1 << layer));
    }
}