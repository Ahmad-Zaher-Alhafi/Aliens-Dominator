using AmmoMagazines;
using Context;
using DG.Tweening;
using ManagersAndControllers;
using Placeables;
using SecurityWeapons;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Extensions;

public class WeaponConstructionPanel : MonoBehaviour, IPlaceableObject, IPointerEnterHandler, IPointerExitHandler {
    [Header("Holders")]
    [SerializeField] private GameObject buttonsHolder;
    [SerializeField] private GameObject buildButtonsHolder;
    [SerializeField] private GameObject secondaryButtonsHolder;

    [Header("Buttons")]
    [SerializeField] private ConstructionButton groundWeaponBuildButton;
    [SerializeField] private ConstructionButton airWeaponBuildButton;
    [SerializeField] private ConstructionButton fighterPlaneBuildWeaponButton;
    [SerializeField] private ConstructionButton repairButton;
    [SerializeField] private ConstructionButton reloadBulletAmmoButton;
    [SerializeField] private ConstructionButton reloadRocketAmmoButton;
    [SerializeField] private ConstructionButton sellButton;

    public GameObject GameObject => gameObject;

    private WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable;
    private bool expandButtons;
    private CanvasGroup buttonsHolderCanvasGroup;

    private void Awake() {
        buttonsHolder.SetActive(false);
        buttonsHolderCanvasGroup = buttonsHolder.GetComponent<CanvasGroup>();

        groundWeaponBuildButton.SetText(SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType.Ground).ToString());
        airWeaponBuildButton.SetText(SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType.Air).ToString());
        fighterPlaneBuildWeaponButton.SetText(SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType.FighterPlane).ToString());

        Ctx.Deps.EventsManager.ViewModeChanged += OnViewModeChanged;
    }

    public void SetPlaceable(AddressablePlaceable placeable) {
        weaponConstructionPanelPlaceable = (WeaponConstructionPanelPlaceable) placeable;

        groundWeaponBuildButton.gameObject.SetActive(weaponConstructionPanelPlaceable.ShowGroundWeaponBuildButton);
        airWeaponBuildButton.gameObject.SetActive(weaponConstructionPanelPlaceable.ShowAirWeaponBuildButton);
        fighterPlaneBuildWeaponButton.gameObject.SetActive(weaponConstructionPanelPlaceable.ShowFighterPlaneBuildButton);
    }

    private void Update() {
        RefreshButtonsVisibility();
        transform.position = weaponConstructionPanelPlaceable.Position;

        reloadBulletAmmoButton.SetText(weaponConstructionPanelPlaceable.BulletsAmountInMagazine + "/" + weaponConstructionPanelPlaceable.BulletsMagazineCapacity,
            weaponConstructionPanelPlaceable.BulletsAmountInMagazine > 0 ? Colors.Instance.Normal : Colors.Instance.RedUI, weaponConstructionPanelPlaceable.BulletsAmountInMagazine <= 0);
        reloadRocketAmmoButton.SetText(weaponConstructionPanelPlaceable.RocketsAmountInMagazine + "/" + weaponConstructionPanelPlaceable.RocketsMagazineCapacity,
            weaponConstructionPanelPlaceable.RocketsAmountInMagazine > 0 ? Colors.Instance.Normal : Colors.Instance.RedUI, weaponConstructionPanelPlaceable.RocketsAmountInMagazine <= 0);

        sellButton.SetText(weaponConstructionPanelPlaceable.RefundAmountFromSellingWeapon.ToString());
        repairButton.SetText(weaponConstructionPanelPlaceable.RepairButtonText, weaponConstructionPanelPlaceable.RepairButtonTextColor);

        reloadBulletAmmoButton.gameObject.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowRefillBulletAmmoButton);
        reloadRocketAmmoButton.gameObject.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowRefillRocketAmmoButton);
    }

    private void OnViewModeChanged(GameController.ViewMode previousMode, GameController.ViewMode currentMode) {
        OnWeaponButtonPointerExit();
    }

    public void BuildGroundWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.Ground)) {
            groundWeaponBuildButton.PlayErrorAnimation();
            return;
        }

        if (!weaponConstructionPanelPlaceable.TryBuildWeapon(DefenceWeapon.WeaponsType.Ground)) return;

        OnPointerEnter(null);
        OnWeaponButtonPointerExit();
    }

    public void BuildAirWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.Air)) {
            airWeaponBuildButton.PlayErrorAnimation();
            return;
        }

        if (!weaponConstructionPanelPlaceable.TryBuildWeapon(DefenceWeapon.WeaponsType.Air)) return;

        OnPointerEnter(null);
        OnWeaponButtonPointerExit();
    }

    public void SellButtonClicked() {
        weaponConstructionPanelPlaceable.BulldozeWeapon();
    }

    public void BuildFighterPlaneWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.FighterPlane)) {
            fighterPlaneBuildWeaponButton.PlayErrorAnimation();
            return;
        }

        if (!weaponConstructionPanelPlaceable.TryBuildWeapon(DefenceWeapon.WeaponsType.FighterPlane)) return;

        OnPointerEnter(null);
        OnWeaponButtonPointerExit();
    }

    public void OnGroundWeaponButtonPointerEnter() {
        weaponConstructionPanelPlaceable.ShowWeaponPlaceholder(DefenceWeapon.WeaponsType.Ground);
    }

    public void OnAirWeaponButtonPointerEnter() {
        weaponConstructionPanelPlaceable.ShowWeaponPlaceholder(DefenceWeapon.WeaponsType.Air);
    }

    public void OnFighterPlaneButtonPointerEnter() {
        weaponConstructionPanelPlaceable.ShowWeaponPlaceholder(DefenceWeapon.WeaponsType.FighterPlane);
    }

    public void RefillBulletAmmoClicked() {
        weaponConstructionPanelPlaceable.RefillAmmo(Magazine.AmmoType.Bullet);
    }

    public void RefillRocketAmmoClicked() {
        weaponConstructionPanelPlaceable.RefillAmmo(Magazine.AmmoType.Rocket);
    }

    public void RepairClicked() {
        if (!weaponConstructionPanelPlaceable.HasAnyConstructionSupplies) {
            repairButton.PlayErrorAnimation();
            return;
        }

        weaponConstructionPanelPlaceable.RepairWeapon();
    }

    public void OnWeaponButtonPointerExit() {
        weaponConstructionPanelPlaceable.HideWeaponPlaceholder();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        expandButtons = true;
        weaponConstructionPanelPlaceable.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        expandButtons = false;
        weaponConstructionPanelPlaceable.OnPointerExit();
    }

    private void RefreshButtonsVisibility() {
        buttonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.IsVisible);

        buttonsHolderCanvasGroup.DOFade(weaponConstructionPanelPlaceable.IsDimmed ? .3f : 1, .5f);
        buttonsHolderCanvasGroup.blocksRaycasts = !weaponConstructionPanelPlaceable.IsDimmed;

        groundWeaponBuildButton.gameObject.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowBuildWeaponButtons);
        repairButton.gameObject.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowWeaponModificationButtons);

        secondaryButtonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowWeaponModificationButtons && expandButtons);
        buildButtonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowBuildWeaponButtons && expandButtons);
    }

    private void OnDestroy() {
        Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
    }
}