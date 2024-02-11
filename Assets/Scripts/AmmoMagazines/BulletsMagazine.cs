using Multiplayer;
using Projectiles;
using Unity.Netcode;
using UnityEngine;

namespace AmmoMagazines {
    public class BulletsMagazine : Magazine {
        public override AmmoType TypeOfAmmo => AmmoType.Bullet;

        [SerializeField] protected Bullet bulletPrefab;

        public override Projectile GetProjectile() {
            if (CurrentProjectilesNumber == 0) return null;

            Bullet bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab.gameObject, default, Quaternion.identity).GetComponent<Bullet>();
            bullet.GetComponent<NetworkObject>().Spawn();
            bullet.InitDefaults(default);
            CurrentProjectilesNumber--;
            return bullet;
        }

        public override void Refill(int projectilesNumberToAdd) {
            CurrentProjectilesNumber = Mathf.Clamp(CurrentProjectilesNumber + projectilesNumberToAdd, 0, capacity);
        }
    }
}