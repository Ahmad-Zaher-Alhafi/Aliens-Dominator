using Placeables;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI {
    public class StateUI : NetworkPlaceableObject {
        [SerializeField] private Slider healthBar;

        private StateUIPlaceable stateUIPlaceable;
        private readonly NetworkVariable<int> networkHealthBarValue = new();
        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<bool> networkShowHealthBar = new();

        public override void SetPlaceable(AddressablePlaceable placeable) {
            base.SetPlaceable(placeable);
            stateUIPlaceable = (StateUIPlaceable) placeable;
            healthBar.minValue = 0;
            healthBar.maxValue = stateUIPlaceable.HealthBarMaxValue;
        }

        private void Update() {
            if (IsServer) {
                healthBar.value = stateUIPlaceable.HealthBarValue;
                networkHealthBarValue.Value = stateUIPlaceable.HealthBarValue;

                transform.position = stateUIPlaceable.Position;
                networkPosition.Value = transform.position;

                healthBar.gameObject.SetActiveWithCheck(stateUIPlaceable.ShowHealthBar);
                networkShowHealthBar.Value = stateUIPlaceable.ShowHealthBar;
            } else {
                healthBar.value = networkHealthBarValue.Value;
                healthBar.gameObject.SetActive(networkShowHealthBar.Value);
                if (networkPosition.Value != Vector3.zero) {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                }
            }
        }
    }
}