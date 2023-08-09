using System.Collections;
using Creatures;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class AirSecurityWeapon : SecurityWeapon<FlyingCreature> {
        [SerializeField] private bool useBursts;
        [SerializeField] private int numOfRocketsInBurst;
        [SerializeField] private float rocketsPerSecondInBurst;
        [SerializeField] private float burstCoolDown = 1.5f;

        private int numOfRocketsShotInBurst;
        private bool isCoolingDown;

        public override float ProjectilesPerSecond {
            get {
                if (useBursts) {
                    return rocketsPerSecondInBurst;
                }

                numOfRocketsShotInBurst = 0;
                return projectilesPerSecond;
            }
        }

        public override float CoolDownTime {
            get {
                if (useBursts && numOfRocketsShotInBurst == numOfRocketsInBurst) {
                    if (!isCoolingDown) {
                        StartCoroutine(CoolDown());
                    }
                    return burstCoolDown;
                }

                return 0;
            }
        }

        public override void Shoot(IDamageable target) {
            Magazine.GetProjectile().Fire(target);
            if (useBursts) {
                numOfRocketsShotInBurst++;
            }
        }

        private IEnumerator CoolDown() {
            isCoolingDown = true;
            yield return new WaitForSeconds(burstCoolDown);
            numOfRocketsShotInBurst = 0;
            isCoolingDown = false;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(AirSecurityWeapon))]
        public class AirSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}