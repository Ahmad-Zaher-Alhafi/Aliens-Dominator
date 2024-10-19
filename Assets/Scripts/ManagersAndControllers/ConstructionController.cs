using System.Collections.Generic;
using System.Linq;
using Context;
using Multiplayer;
using Placeables;
using SecurityWeapons;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace ManagersAndControllers {
    public class ConstructionController : NetworkBehaviour {
        [Header("Weapon prefabs")]
        [SerializeField] private GameObject groundDefenceWeaponPrefab;
        [SerializeField] private GameObject airDefenceWeaponPrefab;
        [SerializeField] private GameObject fighterPlaneWeaponPrefab;

        [Header("Others")]
        [SerializeField] private Transform defenceWeaponsParent;
        [SerializeField] private Transform topDownUI;

        private readonly List<DefenceWeapon> builtWeapons = new();

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
            builtWeapons.Add(defenceWeapon);

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

            DefenceWeapon weaponToBulldoze = weaponConstructionPoint.BuiltWeapon;
            builtWeapons.Remove(weaponToBulldoze);
            weaponToBulldoze.Despawn();
            Ctx.Deps.SuppliesController.PlusSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetRefundAmountFromSellingWeapon(weaponToBulldoze));
            weaponConstructionPoint.OnWeaponDestroyedClientRPC();
        }

        public void BulldozeWeapon(DefenceWeapon defenceWeapon) {
            BulldozeWeapon(weaponConstructionPoints.Single(point => point.BuiltWeapon == defenceWeapon));
        }

        [ServerRpc(RequireOwnership = false)]
        private void BulldozeWeaponServerRPC(NetworkBehaviourReference weaponConstructionPointNetworkReference) {
            NetworkBehaviour weaponConstructionPointNetworkBehaviour = weaponConstructionPointNetworkReference;
            BulldozeWeapon(weaponConstructionPointNetworkBehaviour.gameObject.GetComponent<WeaponConstructionPoint>());
        }

        public void RepairWeapon(DefenceWeapon defenceWeapon) {
            if (!IsServer) {
                RepairWeaponServerRPC(new NetworkBehaviourReference(defenceWeapon));
                return;
            }

            int suppliesToConsume = defenceWeapon.TakenDamage;
            if (!Ctx.Deps.SuppliesController.HasEnoughSupplies(SuppliesController.SuppliesTypes.Construction, defenceWeapon.TakenDamage)) {
                suppliesToConsume = Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.Construction);
            }

            Ctx.Deps.SuppliesController.TryConsumeSupplies(SuppliesController.SuppliesTypes.Construction, suppliesToConsume);
            defenceWeapon.AddHealth(suppliesToConsume);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RepairWeaponServerRPC(NetworkBehaviourReference weaponNetworkReference) {
            NetworkBehaviour weaponNetworkBehaviour = weaponNetworkReference;
            RepairWeapon(weaponNetworkBehaviour.gameObject.GetComponent<DefenceWeapon>());
        }

        public DefenceWeapon GetRandomDefenceWeapon() {
            return MathUtils.GetRandomObjectFromList(builtWeapons);
        }
    }
}