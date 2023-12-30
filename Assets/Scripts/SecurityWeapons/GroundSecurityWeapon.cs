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

        public override Projectile Shoot(IDamageable target) {
            Projectile projectile = base.Shoot(target);
            if (projectile == null) return null;

            projectile.Fire(target, projectileCreatePoint);
            bulletSound.Play();
            return projectile;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeapon))]
        public class GroundSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}