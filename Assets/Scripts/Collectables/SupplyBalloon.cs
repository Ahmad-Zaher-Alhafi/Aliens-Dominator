using Context;
using Unity.Netcode;
using UnityEngine;

namespace Collectables {
    public class SupplyBalloon : NetworkBehaviour {
        [SerializeField] private float heightLimit = 50;
        [SerializeField] private float speed = 3f;
        [Range(1, 100)]
        public int chanceOfSpawning = 50;

        [SerializeField] private Constants.SuppliesTypes suppliesType;

        private readonly NetworkVariable<Vector3> networkPosition = new();

        private void Update() {
            if (IsServer) {
                if (DestroyOnHeightLimitReached()) return;

                transform.position += Vector3.up * speed * Time.deltaTime;
                networkPosition.Value = transform.position;
            } else {
                if (networkPosition.Value != Vector3.zero) {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Arrow")) return;

            Ctx.Deps.EventsManager.TriggerSupplyBalloonCollected(suppliesType);
            Destroy();
        }

        private bool DestroyOnHeightLimitReached() {
            if (transform.position.y >= heightLimit) {
                Destroy();
                return true;
            }

            return false;
        }

        private void Destroy() {
            if (IsServer) {
                Despawn();
            } else {
                DespawnServerRPC();
            }
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