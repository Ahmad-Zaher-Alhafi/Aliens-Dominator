using System.Collections.Generic;
using System.Linq;
using Context;
using ManagersAndControllers;
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

        public override void Refill(int wantedProjectilesNumber, bool consumesSupplies = true) {
            if (!IsServer) {
                RefillServerRPC(wantedProjectilesNumber);
                return;
            }

            int amountToAdd = wantedProjectilesNumber;

            if (consumesSupplies) {
                // Make sure to not exceed the capacity of the magazine
                amountToAdd = CurrentProjectilesNumber + wantedProjectilesNumber > capacity ? capacity - CurrentProjectilesNumber : wantedProjectilesNumber;
                // Make sure that we have enough supplies for the wanted amount, if not then take what is left of the supplies
                amountToAdd = Ctx.Deps.SuppliesController.HasEnoughSupplies(SuppliesController.SuppliesTypes.RocketsAmmo, amountToAdd) ? amountToAdd : Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.RocketsAmmo);
                if (amountToAdd == 0) return;
                if (!Ctx.Deps.SuppliesController.TryConsumeSupplies(SuppliesController.SuppliesTypes.RocketsAmmo, amountToAdd)) return;
            }

            foreach (RocketReloadPoint rocketsReloadPoint in rockets.Keys.ToList()) {
                if (!rocketsReloadPoint.IsUed) continue;
                if (amountToAdd <= 0) break;

                amountToAdd--;
                CurrentProjectilesNumber++;

                rocketsReloadPoint.IsUed = false;

                Rocket rocket = NetworkObjectPool.Singleton.GetNetworkObject(rocketPrefab.gameObject, rocketsReloadPoint.transform.position, rocketsReloadPoint.transform.rotation).GetComponent<Rocket>();
                rocket.GetComponent<NetworkObject>().Spawn();
                rocket.transform.SetParent(transform);
                rocket.InitDefaults();
                rockets[rocketsReloadPoint] = rocket;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RefillServerRPC(int projectilesNumberToAdd) {
            Refill(projectilesNumberToAdd);
        }

        protected override void UnFillMagazine() {
            base.UnFillMagazine();
            foreach (RocketReloadPoint rocketReloadPoint in rockets.Keys) {
                rocketReloadPoint.IsUed = true;
            }
        }
    }
}