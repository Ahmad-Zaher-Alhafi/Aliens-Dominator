using System.Collections;
using Projectiles;
using Unity.Netcode;
using UnityEngine;

namespace AmmoMagazines {
    public abstract class Magazine : NetworkBehaviour {
        public enum AmmoType {
            Bullet,
            Rocket
        }

        public abstract AmmoType TypeOfAmmo { get; }
        public bool IsEmpty => CurrentProjectilesNumber == 0;

        [SerializeField] protected int projectilesNumberOnStart;
        [SerializeField] protected int capacity;
        public int Capacity => capacity;

        public int CurrentProjectilesNumber {
            get => currentProjectilesNumber;
            protected set {
                currentProjectilesNumber = value;
                networkCurrentProjectilesNumber.Value = currentProjectilesNumber;
            }
        }
        private int currentProjectilesNumber;

        private readonly NetworkVariable<int> networkCurrentProjectilesNumber = new();

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            StartCoroutine(RefillDelayed());
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (IsServer) {
                UnFillMagazine();
            }
        }

        private void Update() {
            if (!IsSpawned) return;

            if (!IsServer) {
                CurrentProjectilesNumber = networkCurrentProjectilesNumber.Value;
            }
        }

        /// <summary>
        /// Need to wait a frame until the pool finishes registering the prefabs on spawn
        /// </summary>
        /// <returns></returns>
        private IEnumerator RefillDelayed() {
            yield return new WaitForEndOfFrame();
            Refill(projectilesNumberOnStart, false);
        }

        public abstract Projectile GetProjectile(Transform spawnPoint = null);

        public abstract void Refill(int wantedProjectilesNumber, bool consumesSupplies = true);

        protected virtual void UnFillMagazine() {
            CurrentProjectilesNumber = 0;
        }
    }
}