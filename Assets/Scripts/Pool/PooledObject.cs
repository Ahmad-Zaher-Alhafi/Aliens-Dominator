using UnityEngine;

namespace Pool {
    public class PooledObject : MonoBehaviour {
        public ObjectPool poolRelatedTo;

        public T GetObject<T>(Transform parent) where T : PooledObject {
            if (poolRelatedTo == null) {
                poolRelatedTo = ObjectPool.CreatNewPool(this);
            }

            return (T) poolRelatedTo.GetPooledObject(parent);
        }

        protected void ReturnToPool() {
            poolRelatedTo.AddToPool(this);
        }
    }
}