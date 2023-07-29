using Creatures;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class GroundSecurityWeapon : SecurityWeapon<GroundCreature> {
        protected override void Awake() {
            base.Awake();
            Reload(ammoNumberOnStart);
        }
        public override void Shoot(IDamageable target) {
            GameObject projectile = Instantiate(projectilePrefab, projectileCreatePoint.position, projectilePrefab.transform.rotation);
            projectile.GetComponent<Rigidbody>().AddRelativeForce(transform.forward * 200, ForceMode.Impulse);
            currentAmmoNumber--;
        }

        protected override void Reload(int ammoNumberToAdd) {
            currentAmmoNumber = Mathf.Clamp(currentAmmoNumber + ammoNumberToAdd, 0, maxBulletsNumber);
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeapon))]
        public class GroundSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}