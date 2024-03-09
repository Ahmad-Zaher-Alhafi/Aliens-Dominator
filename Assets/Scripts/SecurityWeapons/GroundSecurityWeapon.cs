using Creatures;
using FMODUnity;
using Projectiles;
using Unity.Netcode;
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
            PlayBulletSoundClientRPC();
            return projectile;
        }

        [ClientRpc]
        private void PlayBulletSoundClientRPC() {
            bulletSound.Play();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeapon))]
        public class GroundSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}