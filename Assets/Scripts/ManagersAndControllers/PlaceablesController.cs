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
            await GetPlaceableObject<TPlaceableObject>(placeable, parent);
        }

        public async void Place<TPlaceableObject>(AddressablePlaceable placeable, Vector3 position, Transform parent) where TPlaceableObject : IPlaceableObject {
            TPlaceableObject placeableObject = await GetPlaceableObject<TPlaceableObject>(placeable, parent);
            placeableObject.GameObject.transform.position = position;
        }

        public async void PlaceOnNetwork<TPlaceableObject>(GameObject prefab, AddressablePlaceable placeable, Transform parent, Transform createRectPoint) where TPlaceableObject : IPlaceableObject {
            TPlaceableObject placeableObject = await GetNetworkPlaceableObject<TPlaceableObject>(prefab, placeable, parent, createRectPoint.position);

            if (placeableObject is NetworkPlaceableObject networkPlaceableObject) {
                networkPlaceableObject.NetworkObject.Spawn();
            }
        }

        private async Task<TPlaceableObject> GetPlaceableObject<TPlaceableObject>(AddressablePlaceable placeable, Transform parent) where TPlaceableObject : IPlaceableObject {
            placeables.Add(placeable);
            GameObject placeableGameObject = await placeable.GetGameObjectAsync(parent);
            TPlaceableObject placeableObject = placeableGameObject.GetComponent<TPlaceableObject>();
            placeableObject.SetPlaceable(placeable);
            return placeableObject;
        }

        /// <summary>
        /// Will try to find the object from the network pool, if not found then a new one will be instantiated
        /// </summary>
        /// <param name="prefab">the prefab of the object that is being created by the plceable</param>
        /// <param name="placeable"></param>
        /// <param name="parent"></param>
        /// <param name="createPosition"></param>
        /// <typeparam name="TPlaceableObject"></typeparam>
        /// <returns></returns>
        private async Task<TPlaceableObject> GetNetworkPlaceableObject<TPlaceableObject>(GameObject prefab, AddressablePlaceable placeable, Transform parent, Vector3 createPosition) where TPlaceableObject : IPlaceableObject {
            placeables.Add(placeable);
            GameObject wantedGameObject = placeable.TryGetGameObjectFromNetworkPool(prefab, createPosition);
            if (wantedGameObject != null) {
                TPlaceableObject networkPlaceableObject = wantedGameObject.GetComponent<TPlaceableObject>();
                networkPlaceableObject.SetPlaceable(placeable);
                return networkPlaceableObject;
            }

            return await GetPlaceableObject<TPlaceableObject>(placeable, parent);
        }

        public void PlaceableDestroyed(AddressablePlaceable placeable) {
            placeables.Remove(placeable);
        }

        public List<T> GetPlaceablesOfType<T>() where T : AddressablePlaceable {
            return placeables.OfType<T>().ToList();
        }
    }
}