using Creatures;
using UnityEditor;

namespace SecurityWeapons {
    public class AirSecurityWeapon : SecurityWeapon<FlyingCreature> {
        public override void Shoot(IDamageable target) {
            Magazine.GetProjectile().Fire(target);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(AirSecurityWeapon))]
        public class AirSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}