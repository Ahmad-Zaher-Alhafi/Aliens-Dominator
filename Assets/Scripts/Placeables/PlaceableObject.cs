using UnityEngine;

namespace Placeables {
    public class PlaceableObject : MonoBehaviour {
        public AddressablePlaceable Placeable { get; protected set; }

        public virtual void SetPlaceable(AddressablePlaceable placeable) {
            Placeable = placeable;
        }
    }
}