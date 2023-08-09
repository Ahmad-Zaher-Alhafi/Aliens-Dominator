using System;
using System.Collections.Generic;
using System.Linq;
using Projectiles;
using UnityEngine;

namespace AmmoMagazines {
    public class RocketsMagazine : Magazine {
        public override Type AmmoType => typeof(Rocket);

        [SerializeField] private List<Transform> rocketsCreatePoints;
        [SerializeField] protected Rocket rocketPrefab;

        private readonly Dictionary<RocketReloadPoint, Rocket> rockets = new();

        protected override void Awake() {
            for (int i = 0; i < capacity; i++) {
                rockets.Add(new RocketReloadPoint(rocketsCreatePoints[i].parent, rocketsCreatePoints[i].localPosition), null);
            }
            base.Awake();
        }

        public override Projectile GetProjectile() {
            KeyValuePair<RocketReloadPoint, Rocket> rocket = rockets.FirstOrDefault(pair => !pair.Key.IsUed);
            if (rocket.Value == null) return null;
            rocket.Key.IsUed = true;
            CurrentProjectilesNumber--;
            return rocket.Value;
        }

        public override void Refill(int projectilesNumberToAdd) {
            foreach (RocketReloadPoint rocketsReloadPoint in rockets.Keys.ToList()) {
                if (!rocketsReloadPoint.IsUed) continue;
                if (projectilesNumberToAdd <= 0) break;

                projectilesNumberToAdd--;
                CurrentProjectilesNumber++;

                rocketsReloadPoint.IsUed = false;

                Rocket rocket = rocketPrefab.GetObject<Rocket>(rocketsReloadPoint.Parent);
                rocket.InitDefaults(rocketsReloadPoint.InitialLocalPosition);
                rockets[rocketsReloadPoint] = rocket;
            }
        }
    }
}