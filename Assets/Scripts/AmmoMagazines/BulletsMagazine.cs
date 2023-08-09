using System;
using Projectiles;
using UnityEngine;

namespace AmmoMagazines {
    public class BulletsMagazine : Magazine {
        public override Type AmmoType => typeof(Bullet);
        
        [SerializeField] protected Bullet bulletPrefab;
        
        public override Projectile GetProjectile() {
            if (CurrentProjectilesNumber == 0) return null;
            Bullet bullet = bulletPrefab.GetObject<Bullet>(null);
            bullet.InitDefaults(default);
            CurrentProjectilesNumber--;
            return bullet;
        }

        public override void Refill(int projectilesNumberToAdd) {
            CurrentProjectilesNumber = Mathf.Clamp(CurrentProjectilesNumber + projectilesNumberToAdd, 0, capacity);
        }
    }
}