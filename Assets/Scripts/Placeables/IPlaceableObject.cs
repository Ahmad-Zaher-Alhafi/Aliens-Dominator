using UnityEngine;

namespace Placeables {
    public interface IPlaceableObject {
        GameObject GameObject { get; }
        void SetPlaceable(AddressablePlaceable placeable);
    }
}