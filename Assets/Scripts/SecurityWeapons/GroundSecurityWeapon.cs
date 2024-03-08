using Creatures;
using FMODUnity;
using Projectiles;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class GroundSecurityWeapon : SecurityWeapon<GroundCreature> {
        [SerializeField] private Transform projectileCreatePoint;

        [Header("Audio files")]
        [SerializeField] private StudioEventEmitter bulletSound;

        public override Projectile Shoot(IDamageable target, Transform spawnPoint = null) {
            Projectile projectile = base.Shoot(target, projectileCreatePoint);
            if (projectile == null) return null;

            projectile.Fire(target);
            bulletSound.Play();
            return projectile;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeapon))]
        public class GroundSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}