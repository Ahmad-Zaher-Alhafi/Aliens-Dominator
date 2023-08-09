using UnityEngine;

namespace Projectiles {
    public class RocketReloadPoint {
        public bool IsUed = true;
        public readonly Vector3 InitialLocalPosition;
        public readonly Transform Parent;

        public RocketReloadPoint(Transform parent, Vector3 initialLocalPosition) {
            Parent = parent;
            InitialLocalPosition = initialLocalPosition;
        }
    }
}