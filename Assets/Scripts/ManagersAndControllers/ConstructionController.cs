using System.Collections.Generic;
using System.Linq;
using Context;
using Multiplayer;
using Placeables;
using SecurityWeapons;
using Unity.Netcode;
using UnityEngine;

namespace ManagersAndControllers {
    public class ConstructionController : NetworkBehaviour {
        [Header("Weapon prefabs")]
        [SerializeField] private GameObject groundDefenceWeaponPrefab;
        [SerializeField] private GameObject airDefenceWeaponPrefab;
        [SerializeField] private GameObject fighterPlaneWeaponPrefab;

        [Header("Others")]
        [SerializeField] private Transform defenceWeaponsParent;
        [SerializeField] private Transform topDownUI;

        private List<WeaponConstructionPoint> weaponConstructionPoints = new();

        private void Awake() {
            weaponConstructionPoints = FindObjectsOfType<WeaponConstructionPoint>().ToList();
            CreateWeaponConstructionPanels(weaponConstructionPoints);
        }

        private void CreateWeaponConstructionPanel(WeaponConstructionPoint weaponConstructionPoint) {
            WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable = new WeaponConstructionPanelPlaceable(weaponConstructionPoint);
            Ctx.Deps.PlaceablesController.Place<WeaponConstructionPanel>(weaponConstructionPanelPlaceable, topDownUI);
        }

        private void CreateWeaponConstructionPanels(List<WeaponConstructionPoint> weaponConstructionPoints) {
            foreach (WeaponConstructionPoint weaponConstructionPoint in weaponConstructionPoints) {
                CreateWeaponConstructionPanel(weaponConstructionPoint);
            }
        }

        public bool TryBuildWeapon(DefenceWeapon.WeaponsType weaponType, WeaponConstructionPoint weaponConstructionPoint, Vector3 position, Quaternion rotation) {
            if (weaponConstructionPoint.IsWeaponBuilt) return false;

            if (IsServer) {
                if (!Ctx.Deps.SuppliesController.TryConsumeSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(weaponType))) return false;
            } else {
                if (!Ctx.Deps.SuppliesController.HasEnoughSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(weaponType))) return false;
                TryBuildWeaponServerRPC(weaponType, new NetworkBehaviourReference(weaponConstructionPoint), position, rotation);
                return true;
            }

            NetworkObject networkObject = weaponType switch {
                DefenceWeapon.WeaponsType.Ground => NetworkObjectPool.Singleton.GetNetworkObject(groundDefenceWeaponPrefab, position, rotation),
                DefenceWeapon.WeaponsType.Air => NetworkObjectPool.Singleton.GetNetworkObject(airDefenceWeaponPrefab, position, rotation),
                DefenceWeapon.WeaponsType.FighterPlane => NetworkObjectPool.Singleton.GetNetworkObject(fighterPlaneWeaponPrefab, position, rotation),
                _ => null
            };

            if (networkObject is null) {
                // Failed to instantiate the weapon, refund the resources
                Ctx.Deps.SuppliesController.PlusSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(weaponType));
                return false;
            }

            networkObject.Spawn(true);
            var defenceWeapon = networkObject.GetComponent<DefenceWeapon>();
            defenceWeapon.Init();
            networkObject.gameObject.transform.SetParent(defenceWeaponsParent, true);

            weaponConstructionPoint.OnWeaponBuiltClientRPC(new NetworkBehaviourReference(defenceWeapon));

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryBuildWeaponServerRPC(DefenceWeapon.WeaponsType weaponType, NetworkBehaviourReference weaponConstructionPointNetworkReference, Vector3 position, Quaternion rotation) {
            NetworkBehaviour constructionPointNetworkBehaviour = weaponConstructionPointNetworkReference;
            TryBuildWeapon(weaponType, constructionPointNetworkBehaviour.GetComponent<WeaponConstructionPoint>(), position, rotation);
        }

        public void BulldozeWeapon(WeaponConstructionPoint weaponConstructionPoint) {
            if (!IsServer) {
                BulldozeWeaponServerRPC(weaponConstructionPoint);
                return;
            }

            weaponConstructionPoint.BuiltWeapon.Despawn();
            Ctx.Deps.SuppliesController.PlusSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetRefundAmountFromSellingWeapon(weaponConstructionPoint.BuiltWeapon.WeaponType));
            weaponConstructionPoint.OnWeaponDestroyedClientRPC();
        }

        [ServerRpc(RequireOwnership = false)]
        private void BulldozeWeaponServerRPC(NetworkBehaviourReference weaponConstructionPointNetworkReference) {
            NetworkBehaviour weaponConstructionPointNetworkBehaviour = weaponConstructionPointNetworkReference;
            BulldozeWeapon(weaponConstructionPointNetworkBehaviour.gameObject.GetComponent<WeaponConstructionPoint>());
        }
    }
}