using System.Collections.Generic;
using Context;
using ManagersAndControllers;
using SecurityWeapons;
using Unity.Netcode;
using UnityEngine;
public class WeaponConstructionPoint : NetworkBehaviour {
    [SerializeField] private List<DefenceWeapon.WeaponsType> weaponTypesThatCanBeBuiltInThisPoint;
    [SerializeField] private GameObject circleEffect;
    [SerializeField] private GameObject airWeaponBase;
    public IReadOnlyList<DefenceWeapon.WeaponsType> WeaponTypesThatCanBeBuiltInThisPoint => weaponTypesThatCanBeBuiltInThisPoint;

    public bool IsWeaponBuilt {
        get => isWeaponBuilt;
        private set {
            isWeaponBuilt = value;
            circleEffect.SetActive(ShowCircleEffect);

            if (isWeaponBuilt) {
                airWeaponBase.SetActive(builtWeapon.WeaponType == DefenceWeapon.WeaponsType.Air);
            }
        }
    }
    private bool isWeaponBuilt;

    private bool ShowCircleEffect => !IsWeaponBuilt && Ctx.Deps.GameController.CurrentViewMode is GameController.ViewMode.TopDown;

    public Vector3 WeaponCreatePosition => transform.position;
    public Quaternion WeaponCreateRotation => transform.rotation;

    private DefenceWeapon builtWeapon;

    private void Awake() {
        Ctx.Deps.EventsManager.ViewModeChanged += OnViewModeChanged;
    }

    private void OnViewModeChanged(GameController.ViewMode previousViewMode, GameController.ViewMode currentViewMode) {
        circleEffect.SetActive(ShowCircleEffect);
    }

    [ClientRpc]
    public void OnWeaponBuiltClientRPC(NetworkBehaviourReference highlightableWeaponNetworkReference) {
        NetworkBehaviour highlightableWeaponNetworkBehaviour = highlightableWeaponNetworkReference;
        builtWeapon = highlightableWeaponNetworkBehaviour.GetComponent<DefenceWeapon>();
        IsWeaponBuilt = true;
    }

    public void OnSelected() {
        if (!IsWeaponBuilt) return;

        builtWeapon.HighlightAsSelected();
    }

    public void OnDeselected() {
        if (!IsWeaponBuilt) return;

        builtWeapon.HighlightNormal();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
    }
}