using UnityEngine;

namespace Pool {
    public class PooledObject : MonoBehaviour {
        public ObjectPool PoolRelatedTo { get; set; }

        public T GetObject<T>(Transform parent) where T : PooledObject {
            if (PoolRelatedTo == null) {
                PoolRelatedTo = ObjectPool.CreatNewPool(this);
            }

            return (T) PoolRelatedTo.GetPooledObject(parent);
        }

        protected void ReturnToPool() {
            PoolRelatedTo.AddToPool(this);
        }
    }
}