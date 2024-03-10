namespace Placeables {
    public class StateUIPlaceable : AddressablePlaceable {
        public int HealthBarValue => damageable.Health;
        public int HealthBarMaxValue { get; }
        public bool ShowHealthBar => !damageable.IsDestroyed;

        private readonly IDamageable damageable;

        public StateUIPlaceable(IDamageable damageable, int healthBarMaxValue) : base("Assets/Prefabs/UI/State UI View.prefab") {
            this.damageable = damageable;
            HealthBarMaxValue = healthBarMaxValue;
        }
    }
}