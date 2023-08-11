using Creatures;
using FMODUnity;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class GroundSecurityWeapon : SecurityWeapon<GroundCreature> {
        [SerializeField] private Transform projectileCreatePoint;
        
        [Header("Audio files")]
        [SerializeField] private StudioEventEmitter bulletSound;

        public override void Shoot(IDamageable target) {
            Magazine.GetProjectile().Fire(target, projectileCreatePoint);
            bulletSound.Play();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeapon))]
        public class GroundSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}