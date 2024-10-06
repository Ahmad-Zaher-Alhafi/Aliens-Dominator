using System.Collections.Generic;
using SecurityWeapons;
using Unity.Netcode;
using UnityEngine;
public class WeaponConstructionPoint : NetworkBehaviour {
    [SerializeField] private List<DefenceWeapon.WeaponType> weaponTypesThatCanBeBuiltInThisPoint;
    [SerializeField] private GameObject circleEffect;
    public IReadOnlyList<DefenceWeapon.WeaponType> WeaponTypesThatCanBeBuiltInThisPoint => weaponTypesThatCanBeBuiltInThisPoint;

    public bool IsWeaponBuilt {
        get => isWeaponBuilt;
        private set {
            circleEffect.SetActive(isWeaponBuilt);
            isWeaponBuilt = value;
        }
    }
    private bool isWeaponBuilt;

    public Vector3 WeaponCreatePosition => transform.position;
    public Quaternion WeaponCreateRotation => transform.rotation;

    [ClientRpc]
    public void OnWeaponBuiltClientRPC() {
        IsWeaponBuilt = true;
    }
}