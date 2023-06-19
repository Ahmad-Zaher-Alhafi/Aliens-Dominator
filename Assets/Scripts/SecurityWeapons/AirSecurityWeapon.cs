using System.Collections.Generic;
using System.Linq;
using Projectiles;
using UnityEngine;

namespace SecurityWeapons {
    public class AirSecurityWeapon : SecurityWeapon {
        private readonly Dictionary<RocketsReloadPoint, Projectile> rockets = new();

        protected override void Awake() {
            base.Awake();
            foreach (Projectile projectile in GetComponentsInChildren<Projectile>()) {
                rockets.Add(new RocketsReloadPoint(projectile.transform.parent, projectile.transform.position), projectile);
            }
        }

        public override void Shoot(IDamageable target) {
            KeyValuePair<RocketsReloadPoint, Projectile> projectile = rockets.FirstOrDefault(pair => !pair.Key.isUed);
            if (projectile.Value == null) return;

            projectile.Key.isUed = true;
            projectile.Value.FollowTarget(target);
        }

        public override void Reload(int ammoNumber) {
            foreach (RocketsReloadPoint rocketsReloadPoint in rockets.Keys) {
                if (!rocketsReloadPoint.isUed) return;
                if (ammoNumber <= 0) return;

                ammoNumber--;
                
                rocketsReloadPoint.isUed = false;
                
                rockets[rocketsReloadPoint] = Instantiate(projectilePrefab, rocketsReloadPoint.Parent).GetComponent<Projectile>();
                rockets[rocketsReloadPoint].transform.localScale = Vector3.one;
                rockets[rocketsReloadPoint].transform.localEulerAngles = Vector3.zero;
                rockets[rocketsReloadPoint].transform.position = rocketsReloadPoint.InitialPosition;
            }

            UpdateAmmoStateText();
        }


        private class RocketsReloadPoint {
            public bool isUed;
            public readonly Vector3 InitialPosition;
            public readonly Transform Parent;

            public RocketsReloadPoint(Transform parent, Vector3 initialPosition) {
                Parent = parent;
                InitialPosition = initialPosition;
            }
        }
    }

}