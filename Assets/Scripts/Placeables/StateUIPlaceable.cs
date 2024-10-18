using UnityEngine;

namespace Placeables {
    public class StateUIPlaceable : AddressablePlaceable {
        public int HealthBarValue => damageable.Health;
        public int HealthBarMaxValue { get; }
        public bool ShowHealthBar => !damageable.IsDestroyed;
        public Vector3 Position => pointToFollow.position;
        public Color BackgroundColor { get; }
        public Color FillColor { get; }

    private readonly IDamageable damageable;
        private readonly Transform pointToFollow;

        public StateUIPlaceable(IDamageable damageable, int healthBarMaxValue, Transform pointToFollow, Color backgroundColor, Color fillColor) : base("Assets/Prefabs/UI/State UI View.prefab") {
            this.damageable = damageable;
            this.pointToFollow = pointToFollow;
            HealthBarMaxValue = healthBarMaxValue;
            BackgroundColor = backgroundColor;
            FillColor = fillColor;
        }
    }
}