using Unity.Netcode;
using UnityEngine;

namespace Collectables {
    public abstract class Supplies : NetworkBehaviour {
        [SerializeField] private Constants.SuppliesTypes suppliesType;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private void Update() {
            if (IsServer) {
                networkPosition.Value = transform.position;
                networkRotation.Value = transform.rotation;
            } else {
                if (networkPosition.Value == Vector3.zero) return;

                transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.gameObject.CompareTag("Arrow")) return;

            OnCollected(suppliesType);

            if (IsServer) {
                Despawn();
            } else {
                DespawnServerRPC();
            }
        }

        protected abstract void OnCollected(Constants.SuppliesTypes suppliesType);

        private void Despawn() {
            gameObject.SetActive(false);
            NetworkObject.Despawn(false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRPC() {
            Destroy(gameObject);
        }
    }
}