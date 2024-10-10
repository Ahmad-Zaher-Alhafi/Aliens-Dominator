﻿using Context;
using ManagersAndControllers;
using Multiplayer;
using Projectiles;
using Unity.Netcode;
using UnityEngine;

namespace AmmoMagazines {
    public class BulletsMagazine : Magazine {
        public override AmmoType TypeOfAmmo => AmmoType.Bullet;

        [SerializeField] protected Bullet bulletPrefab;

        public override Projectile GetProjectile(Transform spawnPoint = null) {
            if (CurrentProjectilesNumber == 0) return null;

            Bullet bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab.gameObject, spawnPoint.position, spawnPoint.rotation).GetComponent<Bullet>();
            bullet.GetComponent<NetworkObject>().Spawn();
            bullet.InitDefaults();
            CurrentProjectilesNumber--;
            return bullet;
        }

        public override void Refill(int projectilesNumberToAdd, bool consumesSupplies = true) {
            if (!IsServer) {
                RefillServerRPC(projectilesNumberToAdd);
                return;
            }

            if (!consumesSupplies) {
                CurrentProjectilesNumber = Mathf.Clamp(CurrentProjectilesNumber + projectilesNumberToAdd, 0, capacity);
                return;
            }

            // Make sure to not exceed the capacity of the magazine
            int amountToAdd = CurrentProjectilesNumber + projectilesNumberToAdd > capacity ? capacity - CurrentProjectilesNumber : projectilesNumberToAdd;
            // Make sure that we have enough supplies for the wanted amount, if not then take what is left of the supplies
            amountToAdd = Ctx.Deps.SuppliesController.HasEnoughSupplies(SuppliesController.SuppliesTypes.BulletsAmmo, amountToAdd) ? amountToAdd : Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.BulletsAmmo);
            if (!Ctx.Deps.SuppliesController.TryConsumeSupplies(SuppliesController.SuppliesTypes.BulletsAmmo, amountToAdd)) return;
            CurrentProjectilesNumber = Mathf.Clamp(CurrentProjectilesNumber + amountToAdd, 0, capacity);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RefillServerRPC(int projectilesNumberToAdd) {
            Refill(projectilesNumberToAdd);
        }
    }
}