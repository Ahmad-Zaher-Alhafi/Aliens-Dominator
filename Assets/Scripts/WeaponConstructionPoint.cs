using System.Collections.Generic;
using SecurityWeapons;
using Unity.Netcode;
using UnityEngine;
public class WeaponConstructionPoint : NetworkBehaviour {
    [SerializeField] private List<DefenceWeapon.WeaponType> weaponTypesThatCanBeBuiltInThisPoint;
    public IReadOnlyList<DefenceWeapon.WeaponType> WeaponTypesThatCanBeBuiltInThisPoint => weaponTypesThatCanBeBuiltInThisPoint;

    public Vector3 WeaponCreatePosition => transform.position;
    public Quaternion WeaponCreateRotation => transform.rotation;
}