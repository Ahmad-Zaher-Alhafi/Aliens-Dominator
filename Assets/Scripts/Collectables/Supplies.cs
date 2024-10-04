using Context;
using ManagersAndControllers;
using Unity.Netcode;
using UnityEngine;

namespace Collectables {
    public abstract class Supplies : NetworkBehaviour {
        [SerializeField] private SuppliesController.SuppliesTypes suppliesType;
        [SerializeField] protected int amountOfSuppliesInThePack;

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

            if (IsServer) {
                OnCollected();
            } else {
                CollectServerRPC();
            }
        }

        private void OnCollected() {
            if (IsServer) {
                Ctx.Deps.SuppliesController.PlusSupplies(suppliesType, amountOfSuppliesInThePack);
                Despawn();
            } else {
                DespawnServerRPC();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void CollectServerRPC() {
            OnCollected();
        }

        private void Despawn() {
            NetworkObject.Despawn();
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRPC() {
            Despawn();
        }
    }
}