using System.Collections.Generic;
using System.Linq;
using Multiplayer;
using Projectiles;
using Unity.Netcode;
using UnityEngine;

namespace AmmoMagazines {
    public class RocketsMagazine : Magazine {
        public override AmmoType TypeOfAmmo => AmmoType.Rocket;

        [SerializeField] private List<RocketReloadPoint> rocketsCreatePoints;
        [SerializeField] protected Rocket rocketPrefab;

        private readonly Dictionary<RocketReloadPoint, Rocket> rockets = new();

        private void Awake() {
            // Create the reload points
            for (int i = 0; i < capacity; i++) {
                rockets.Add(rocketsCreatePoints[i], null);
            }
        }

        public override Projectile GetProjectile(Transform spawnPoint = null) {
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

                Rocket rocket = NetworkObjectPool.Singleton.GetNetworkObject(rocketPrefab.gameObject, rocketsReloadPoint.transform.position, rocketsReloadPoint.transform.rotation).GetComponent<Rocket>();
                rocket.GetComponent<NetworkObject>().Spawn();
                rocket.transform.SetParent(transform);
                rocket.InitDefaults();
                rockets[rocketsReloadPoint] = rocket;
            }
        }
    }
}