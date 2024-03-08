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

        protected int CurrentProjectilesNumber;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            StartCoroutine(RefillDelayed());
        }

        /// <summary>
        /// Need to wait a frame until the pool finishes registering the prefabs on spawn
        /// </summary>
        /// <returns></returns>
        private IEnumerator RefillDelayed() {
            yield return new WaitForEndOfFrame();
            Refill(projectilesNumberOnStart);
        }

        public abstract Projectile GetProjectile(Transform spawnPoint = null);

        public abstract void Refill(int projectilesNumberToAdd);
    }
}