using Unity.Netcode;
using UnityEngine;

namespace Placeables {
    public class NetworkPlaceableObject : NetworkBehaviour, IPlaceableObject {
        public AddressablePlaceable Placeable { get; protected set; }
        public GameObject GameObject => gameObject;

        public virtual void SetPlaceable(AddressablePlaceable placeable) {
            Placeable = placeable;
        }
        public virtual void BeforeDespawn() { }
    }
}