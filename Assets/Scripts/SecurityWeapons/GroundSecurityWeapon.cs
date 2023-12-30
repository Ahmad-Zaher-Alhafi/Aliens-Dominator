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

        public override void Shoot(IDamageable target) {
            Projectile projectile = Magazine.GetProjectile();
            if (projectile == null) {
                Debug.Log($"Weapon {gameObject.name} ran out of ammo!", gameObject);
                return;
            }
            projectile.Fire(target, projectileCreatePoint);
            bulletSound.Play();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeapon))]
        public class GroundSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}