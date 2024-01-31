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
            if (transform.position.y >= heightLimit) {
                Destroy();
            } else {
                if (IsServer) {
                    transform.position += Vector3.up * speed * Time.deltaTime;
                    networkPosition.Value = transform.position;
                } else {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Arrow")) return;

            Ctx.Deps.EventsManager.OnCallingSupplies(suppliesType);
            Destroy();
        }

        private void Destroy() {
            gameObject.SetActive(false);

            if (IsServer) {
                NetworkObject.Despawn();
            } else {
                DespawnServerRPC();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRPC() {
            NetworkObject.Despawn(false);
        }
    }
}