using System.Collections.Generic;
using UnityEngine;

namespace Pool {
    public class ObjectPool : MonoBehaviour {
        private PooledObject objectPrefab;
        private readonly List<PooledObject> pooledObjects = new();

        public void Init(PooledObject objectPrefab) {
            this.objectPrefab = objectPrefab;
        }

        public static ObjectPool CreatNewPool(PooledObject prefab) {
            GameObject poolObject = new GameObject(prefab.name + " Pool");
            ObjectPool pool = poolObject.AddComponent<ObjectPool>();
            pool.objectPrefab = prefab;
            return pool;
        }

        public PooledObject GetPooledObject(Transform parent) {
            PooledObject pooledObject;

            if (pooledObjects.Count > 0) {
                pooledObject = pooledObjects[0];
                pooledObjects.Remove(pooledObject);
                pooledObject.transform.parent = parent;
                pooledObject.gameObject.SetActive(true);
            } else {
                pooledObject = Instantiate(objectPrefab, parent);
                pooledObject.PoolRelatedTo = this;
            }

            return pooledObject;
        }

        public void AddToPool(PooledObject objectToReturnToPool) {
            objectToReturnToPool.gameObject.SetActive(false);
            objectToReturnToPool.transform.SetParent(transform);
            pooledObjects.Add(objectToReturnToPool);
        }
    }
}