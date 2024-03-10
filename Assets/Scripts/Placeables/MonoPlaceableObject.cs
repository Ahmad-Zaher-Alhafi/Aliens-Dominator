using UnityEngine;

namespace Placeables {
    public class MonoPlaceableObject : MonoBehaviour, IPlaceableObject {
        public AddressablePlaceable Placeable { get; protected set; }
        public GameObject GameObject => gameObject;

        public virtual void SetPlaceable(AddressablePlaceable placeable) {
            Placeable = placeable;
        }
    }
}