using UnityEngine;

namespace SecurityWeapons {
    public class GroundSecurityWeapon : SecurityWeapon {
        public override void Shoot(IDamageable target) {
            GameObject projectile = Instantiate(projectilePrefab, projectileCreatePoint.position, projectilePrefab.transform.rotation);
            projectile.GetComponent<Rigidbody>().AddRelativeForce(transform.forward * 200, ForceMode.Impulse);
        }

        public override void Reload(int ammoNumber) {
            currentBulletsNumber = Mathf.Clamp(currentBulletsNumber + ammoNumber, 0, maxBulletsNumber);
            UpdateAmmoStateText();
        }
    }
}