using Context;
using Unity.Netcode;
using UnityEngine;

namespace Pool {
    public class PooledObject : NetworkBehaviour {
        public ObjectPool PoolRelatedTo { get; set; }
        protected NetworkObject NetworkObject { get; private set; }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            NetworkObject = GetComponent<NetworkObject>();
        }

        public T GetObject<T>(Transform parent, ulong ownerId) where T : PooledObject {
            if (PoolRelatedTo == null) {
                PoolRelatedTo = Ctx.Deps.ObjectPoolController.CreatNewPool(this, ownerId);
            }

            var pooledObject = (T) PoolRelatedTo.GetPooledObject(parent);
            NetworkObject networkObject = pooledObject.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(ownerId);
            networkObject.transform.SetParent(parent);
            return pooledObject;
        }

        public T GetObject<T>(Transform parent) where T : PooledObject {
            if (PoolRelatedTo == null) {
                PoolRelatedTo = ObjectPool.CreatNewPool(this);
            }

            var pooledObject = (T) PoolRelatedTo.GetPooledObject(parent);
            return pooledObject;
        }

        protected void ReturnToPool() {
            PoolRelatedTo.AddToPool(this);
            NetworkObject.Despawn(false);
        }
    }
}