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
    [SerializeField] private RangeVisualizer rangeVisualizer;
    public IReadOnlyList<DefenceWeapon.WeaponsType> WeaponTypesThatCanBeBuiltInThisPoint => weaponTypesThatCanBeBuiltInThisPoint;

    public bool IsWeaponBuilt => BuiltWeapon != null;

    private bool ShowCircleEffect => !IsWeaponBuilt && Ctx.Deps.GameController.CurrentViewMode is GameController.ViewMode.TopDown;

    public Vector3 WeaponCreatePosition => transform.position;
    public Quaternion Rotation => transform.rotation;
    public Quaternion WeaponCreateRotation => WeaponPlaceholder.Instance.ActivePlaceholderRotation;

    public DefenceWeapon BuiltWeapon {
        get => builtWeapon;
        private set {
            builtWeapon = value;
            circleEffect.SetActive(ShowCircleEffect);

            if (builtWeapon != null) {
                airWeaponBase.SetActive(builtWeapon.WeaponType == DefenceWeapon.WeaponsType.Air);
            }
        }
    }
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
        BuiltWeapon = highlightableWeaponNetworkBehaviour.GetComponent<DefenceWeapon>();
    }

    public void OnSelected() {
        if (!IsWeaponBuilt) return;

        BuiltWeapon.HighlightAsSelected();
        rangeVisualizer.ShowRange(WeaponCreatePosition, SharedWeaponSpecifications.Instance.GetWeaponRange(builtWeapon.WeaponType));
    }

    public void OnDeselected() {
        if (!IsWeaponBuilt) return;

        BuiltWeapon.HighlightNormal();
        rangeVisualizer.HideRange();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
    }
}