using System.Collections.Generic;
using System.Threading.Tasks;
using Context;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Placeables {
    public class AddressablePlaceable {
        private readonly string address;
        private static readonly Dictionary<string, Task<GameObject>> prefabsCache = new();
        public GameObject GameObject { get; private set; }

        protected AddressablePlaceable(string address) {
            this.address = address;
        }

        public async Task<GameObject> GetGameObjectAsync(Transform parent) {
            // Invalidate cache if the prefabs are already destroyed. Assume that if one is destroyed all are destroyed
            if (prefabsCache.ContainsKey(address)) {
                Task<GameObject> task = prefabsCache[address];
                if (task.IsCompletedSuccessfully && task.Result == null) prefabsCache.Clear();
            }

            if (!prefabsCache.ContainsKey(address)) {
                var handle = Addressables.LoadAssetAsync<GameObject>(address);
                await handle.Task;
                prefabsCache[address] = handle.Task;
            }

            Task<GameObject> prefabLoadingTask = prefabsCache[address];
            GameObject prefab = await prefabLoadingTask;

            Task<GameObject> instantiatingTask = new Task<GameObject>(() => Object.Instantiate(prefab, parent));
            instantiatingTask.Start(TaskScheduler.FromCurrentSynchronizationContext());
            GameObject = instantiatingTask.Result;

            return GameObject;
        }

        public void Destroy() {
            Object.Destroy(GameObject);
            GameObject = null;
            Ctx.Deps.PlaceablesController.PlaceableDestroyed(this);
        }
    }
}