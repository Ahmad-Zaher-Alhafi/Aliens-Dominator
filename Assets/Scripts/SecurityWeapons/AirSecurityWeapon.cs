using System.Collections.Generic;
using System.Linq;
using Creatures;
using Projectiles;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class AirSecurityWeapon : SecurityWeapon<FlyingCreature> {
        [SerializeField]
        private List<Transform> projectileCreatePoints;
        private readonly Dictionary<RocketsReloadPoint, Projectile> rockets = new();

        protected override void Awake() {
            base.Awake();
            Init();
        }

        private void Init() {
            for (int i = 0; i < ammoNumberOnStart; i++) {
                rockets.Add(new RocketsReloadPoint(projectileCreatePoints[i].parent, projectileCreatePoints[i].localPosition), null);
            }
            Reload(ammoNumberOnStart);
        }

        public override void Shoot(IDamageable target) {
            KeyValuePair<RocketsReloadPoint, Projectile> projectile = rockets.FirstOrDefault(pair => !pair.Key.isUed);
            if (projectile.Value == null) return;

            projectile.Key.isUed = true;
            projectile.Value.FollowTarget(target);
            currentAmmoNumber--;
        }

        protected override void Reload(int ammoNumberToAdd) {
            foreach (RocketsReloadPoint rocketsReloadPoint in rockets.Keys.ToList()) {
                if (!rocketsReloadPoint.isUed) continue;
                if (ammoNumberToAdd <= 0) break;

                ammoNumberToAdd--;
                currentAmmoNumber++;

                rocketsReloadPoint.isUed = false;

                rockets[rocketsReloadPoint] = Instantiate(projectilePrefab, rocketsReloadPoint.Parent).GetComponent<Projectile>();
                rockets[rocketsReloadPoint].transform.localScale = Vector3.one;
                rockets[rocketsReloadPoint].transform.localEulerAngles = Vector3.zero;
                rockets[rocketsReloadPoint].transform.localPosition = rocketsReloadPoint.InitialLocalPosition;
            }
        }


        private class RocketsReloadPoint {
            public bool isUed = true;
            public readonly Vector3 InitialLocalPosition;
            public readonly Transform Parent;

            public RocketsReloadPoint(Transform parent, Vector3 initialLocalPosition) {
                Parent = parent;
                InitialLocalPosition = initialLocalPosition;
            }
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(AirSecurityWeapon))]
        public class AirSecurityWeaponEditor : SecurityWeaponEditor { }
#endif
    }
}