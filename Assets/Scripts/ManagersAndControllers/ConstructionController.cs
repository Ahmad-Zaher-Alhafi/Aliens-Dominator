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

        public void BuildWeapon(DefenceWeapon.WeaponType weaponType, WeaponConstructionPoint weaponConstructionPoint) {
            if (!IsServer) {
                BuildWeaponServerRPC(weaponType, new NetworkBehaviourReference(weaponConstructionPoint));
                return;
            }

            NetworkObject networkObject = weaponType switch {
                DefenceWeapon.WeaponType.Ground => NetworkObjectPool.Singleton.GetNetworkObject(groundDefenceWeaponPrefab, weaponConstructionPoint.WeaponCreatePosition, weaponConstructionPoint.WeaponCreateRotation),
                DefenceWeapon.WeaponType.Air => NetworkObjectPool.Singleton.GetNetworkObject(airDefenceWeaponPrefab, weaponConstructionPoint.WeaponCreatePosition, weaponConstructionPoint.WeaponCreateRotation),
                DefenceWeapon.WeaponType.FighterPlane => NetworkObjectPool.Singleton.GetNetworkObject(fighterPlaneWeaponPrefab, weaponConstructionPoint.WeaponCreatePosition, weaponConstructionPoint.WeaponCreateRotation),
                _ => null
            };

            if (networkObject is null) return;

            networkObject.Spawn(true);
            networkObject.GetComponent<DefenceWeapon>().Init();
            networkObject.gameObject.transform.SetParent(defenceWeaponsParent, true);

            weaponConstructionPoint.OnWeaponBuiltClientRPC();
        }

        [ServerRpc(RequireOwnership = false)]
        private void BuildWeaponServerRPC(DefenceWeapon.WeaponType weaponType, NetworkBehaviourReference weaponConstructionPointNetworkReference) {
            NetworkBehaviour constructionPointNetworkBehaviour = weaponConstructionPointNetworkReference;
            BuildWeapon(weaponType, constructionPointNetworkBehaviour.GetComponent<WeaponConstructionPoint>());
        }
    }
}