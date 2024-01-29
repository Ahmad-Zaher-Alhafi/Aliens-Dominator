using System.Collections.Generic;
using System.Linq;
using Placeables;
using UnityEngine;

namespace ManagersAndControllers {
    public class PlaceablesController : MonoBehaviour {
        public IReadOnlyList<AddressablePlaceable> Placeables => placeables;
        private readonly List<AddressablePlaceable> placeables = new();

        public async void Place(AddressablePlaceable placeable, Transform parent) {
            placeables.Add(placeable);
            GameObject placeableObject = await placeable.GetGameObjectAsync(parent);
            placeableObject.GetComponent<PlaceableObject>().SetPlaceable(placeable);
        }

        public void PlaceableDestroyed(AddressablePlaceable placeable) {
            placeables.Remove(placeable);
        }

        public List<T> GetPlaceablesOfType<T>() where T : AddressablePlaceable {
            return placeables.OfType<T>().ToList();
        }
    }
}