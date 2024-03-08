using Multiplayer;
using Projectiles;
using Unity.Netcode;
using UnityEngine;

namespace AmmoMagazines {
    public class BulletsMagazine : Magazine {
        public override AmmoType TypeOfAmmo => AmmoType.Bullet;

        [SerializeField] protected Bullet bulletPrefab;

        public override Projectile GetProjectile(Transform spawnPoint = null) {
            if (CurrentProjectilesNumber == 0) return null;

            Bullet bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab.gameObject, spawnPoint.position, spawnPoint.rotation).GetComponent<Bullet>();
            bullet.GetComponent<NetworkObject>().Spawn();
            bullet.InitDefaults();
            CurrentProjectilesNumber--;
            return bullet;
        }

        public override void Refill(int projectilesNumberToAdd) {
            CurrentProjectilesNumber = Mathf.Clamp(CurrentProjectilesNumber + projectilesNumberToAdd, 0, capacity);
        }
    }
}