using System.Linq;
using Placeables;
using SecurityWeapons;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Extensions;

public class WeaponConstructionPanel : MonoBehaviour, IPlaceableObject, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private GameObject buttonsHolder;

    [Header("Secondary buttons")]
    [SerializeField] private GameObject repairButton;
    [SerializeField] private GameObject secondaryButtonsHolder;

    [Header("Build buttons")]
    [SerializeField] private GameObject buildButtonsHolder;
    [SerializeField] private GameObject groundBuildWeaponButton;
    [SerializeField] private GameObject airBuildWeaponButton;
    [SerializeField] private GameObject fighterBuildPlaneWeaponButton;

    public GameObject GameObject => gameObject;

    private WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable;
    private bool areSecondaryButtonsShown;
    private bool areBuildButtonsShown;

    private void Awake() {
        buttonsHolder.SetActive(false);
    }

    public void SetPlaceable(AddressablePlaceable placeable) {
        weaponConstructionPanelPlaceable = (WeaponConstructionPanelPlaceable) placeable;

        groundBuildWeaponButton.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponType.Ground));
        airBuildWeaponButton.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponType.Air));
        fighterBuildPlaneWeaponButton.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponType.FighterPlane));
    }

    private void Update() {
        buttonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.IsVisible);
        groundBuildWeaponButton.SetActiveWithCheck(!weaponConstructionPanelPlaceable.IsBuilt);
        repairButton.SetActiveWithCheck(weaponConstructionPanelPlaceable.IsBuilt);
    }

    public void BuildGroundWeaponButtonClicked() {
        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponType.Ground);
        OnPointerEnter(null);
    }

    public void BuildAirWeaponButtonClicked() {
        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponType.Air);
        OnPointerEnter(null);
    }

    public void BuildFighterPlaneWeaponButtonClicked() {
        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponType.FighterPlane);
        OnPointerEnter(null);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (weaponConstructionPanelPlaceable.IsBuilt) {
            areSecondaryButtonsShown = true;
            secondaryButtonsHolder.SetActive(areSecondaryButtonsShown);
            buildButtonsHolder.SetActive(!areSecondaryButtonsShown);
        } else {
            areBuildButtonsShown = true;
            buildButtonsHolder.SetActive(areBuildButtonsShown);
            secondaryButtonsHolder.SetActive(!areBuildButtonsShown);
        }

        weaponConstructionPanelPlaceable.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (weaponConstructionPanelPlaceable.IsBuilt) {
            areSecondaryButtonsShown = false;
            secondaryButtonsHolder.SetActive(areSecondaryButtonsShown);
        } else {
            areBuildButtonsShown = false;
            buildButtonsHolder.SetActive(areBuildButtonsShown);
        }

        weaponConstructionPanelPlaceable.OnPointerExit();
    }
}