using Pool;
using Unity.Netcode;
using UnityEngine;

namespace ManagersAndControllers {
    public class ObjectPoolController : MonoBehaviour {
        [SerializeField] private ObjectPool objectPoolPrefab;

        public ObjectPool CreatNewPool(PooledObject prefab, ulong ownerId) {
            GameObject poolObject = Instantiate(objectPoolPrefab).gameObject;
            poolObject.name = prefab.name + " Pool";
            ObjectPool pool = poolObject.GetComponent<ObjectPool>();
            pool.Init(prefab);
            pool.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
            return pool;
        }
    }
}