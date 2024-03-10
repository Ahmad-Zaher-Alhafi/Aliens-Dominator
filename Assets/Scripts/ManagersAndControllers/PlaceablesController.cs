using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Placeables;
using UnityEngine;

namespace ManagersAndControllers {
    public class PlaceablesController : MonoBehaviour {
        public IReadOnlyList<AddressablePlaceable> Placeables => placeables;
        private readonly List<AddressablePlaceable> placeables = new();

        public async void Place<TPlaceableObject>(AddressablePlaceable placeable, Transform parent) where TPlaceableObject : IPlaceableObject {
            TPlaceableObject placeableObject = await PlaceInternal<TPlaceableObject>(placeable, parent);

            if (placeableObject.GameObject.GetComponent<TPlaceableObject>() is NetworkPlaceableObject networkPlaceableObject) {
                networkPlaceableObject.NetworkObject.Spawn();
            }
        }

        public async void Place<TPlaceableObject>(AddressablePlaceable placeable, Transform parent, Transform createRectPoint) where TPlaceableObject : IPlaceableObject {
            TPlaceableObject placeableObject = await PlaceInternal<TPlaceableObject>(placeable, parent);

            if (placeableObject is NetworkPlaceableObject networkPlaceableObject) {
                networkPlaceableObject.NetworkObject.Spawn();
            }

            if (createRectPoint != null) {
                placeableObject.GameObject.transform.position = createRectPoint.position;
                placeableObject.GameObject.transform.rotation = transform.rotation;
            }
        }

        private async Task<TPlaceableObject> PlaceInternal<TPlaceableObject>(AddressablePlaceable placeable, Transform parent) where TPlaceableObject : IPlaceableObject {
            placeables.Add(placeable);
            GameObject placeableGameObject = await placeable.GetGameObjectAsync(parent);
            TPlaceableObject placeableObject = placeableGameObject.GetComponent<TPlaceableObject>();
            placeableObject.SetPlaceable(placeable);
            return placeableObject;
        }

        public void PlaceableDestroyed(AddressablePlaceable placeable) {
            placeables.Remove(placeable);
        }

        public List<T> GetPlaceablesOfType<T>() where T : AddressablePlaceable {
            return placeables.OfType<T>().ToList();
        }
    }
}