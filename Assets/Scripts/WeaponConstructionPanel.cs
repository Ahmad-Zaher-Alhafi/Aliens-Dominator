using System.Linq;
using DG.Tweening;
using Placeables;
using SecurityWeapons;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
    }

    public void SetPlaceable(AddressablePlaceable placeable) {
        weaponConstructionPanelPlaceable = (WeaponConstructionPanelPlaceable) placeable;

        groundWeaponBuildButton.gameObject.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponsType.Ground));
        airWeaponBuildButton.gameObject.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponsType.Air));
        fighterPlaneBuildWeaponButton.gameObject.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponsType.FighterPlane));
    }

    private void Update() {
        RefreshButtonsVisibility();
        transform.position = weaponConstructionPanelPlaceable.Position;
    }

    public void BuildGroundWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.Ground)) {
            groundWeaponBuildButton.PlayErrorAnimation();
            return;
        }

        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.Ground);
        OnPointerEnter(null);
    }

    public void BuildAirWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.Air)) {
            airWeaponBuildButton.PlayErrorAnimation();
            return;
        }

        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.Air);
        OnPointerEnter(null);
        OnWeaponButtonPointerExit();
    }

    public void BuildFighterPlaneWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.FighterPlane)) {
            fighterPlaneBuildWeaponButton.PlayErrorAnimation();
            return;
        }

        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.FighterPlane);
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
}