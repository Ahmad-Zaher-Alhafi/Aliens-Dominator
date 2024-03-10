using Placeables;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI {
    public class StateUI : PlaceableObject {
        [SerializeField] private Slider healthBar;

        private StateUIPlaceable stateUIPlaceable;

        public override void SetPlaceable(AddressablePlaceable placeable) {
            base.SetPlaceable(placeable);
            stateUIPlaceable = (StateUIPlaceable) placeable;
            healthBar.minValue = 0;
            healthBar.maxValue = stateUIPlaceable.HealthBarMaxValue;
        }

        private void Update() {
            healthBar.value = stateUIPlaceable.HealthBarValue;
            healthBar.gameObject.SetActiveWithCheck(stateUIPlaceable.ShowHealthBar);
        }
    }
}