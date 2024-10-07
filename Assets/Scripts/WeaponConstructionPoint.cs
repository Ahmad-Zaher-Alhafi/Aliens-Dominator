using System.Collections.Generic;
using Context;
using ManagersAndControllers;
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
            isWeaponBuilt = value;
            circleEffect.SetActive(ShowCircleEffect);
        }
    }
    private bool isWeaponBuilt;

    private bool ShowCircleEffect => !IsWeaponBuilt && Ctx.Deps.GameController.CurrentViewMode is GameController.ViewMode.TopDown;

    public Vector3 WeaponCreatePosition => transform.position;
    public Quaternion WeaponCreateRotation => transform.rotation;

    private void Awake() {
        Ctx.Deps.EventsManager.ViewModeChanged += OnViewModeChanged;
    }

    private void OnViewModeChanged(GameController.ViewMode previousViewMode, GameController.ViewMode currentViewMode) {
        circleEffect.SetActive(ShowCircleEffect);
    }

    [ClientRpc]
    public void OnWeaponBuiltClientRPC() {
        IsWeaponBuilt = true;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
    }
}