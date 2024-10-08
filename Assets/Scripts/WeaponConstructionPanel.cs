using System.Linq;
using DG.Tweening;
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
    private bool expandButtons;
    private CanvasGroup buttonsHolderCanvasGroup;

    private void Awake() {
        buttonsHolder.SetActive(false);
        buttonsHolderCanvasGroup = buttonsHolder.GetComponent<CanvasGroup>();
    }

    public void SetPlaceable(AddressablePlaceable placeable) {
        weaponConstructionPanelPlaceable = (WeaponConstructionPanelPlaceable) placeable;

        groundBuildWeaponButton.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponsType.Ground));
        airBuildWeaponButton.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponsType.Air));
        fighterBuildPlaneWeaponButton.SetActive(weaponConstructionPanelPlaceable.WeaponTypesToShow.Contains(DefenceWeapon.WeaponsType.FighterPlane));
    }

    private void Update() {
        RefreshButtonsVisibility();
        transform.position = weaponConstructionPanelPlaceable.Position;
    }

    public void BuildGroundWeaponButtonClicked() {
        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.Ground);
        OnPointerEnter(null);
    }

    public void BuildAirWeaponButtonClicked() {
        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.Air);
        OnPointerEnter(null);
    }

    public void BuildFighterPlaneWeaponButtonClicked() {
        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.FighterPlane);
        OnPointerEnter(null);
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

        groundBuildWeaponButton.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowBuildWeaponButtons);
        repairButton.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowWeaponModificationButtons);

        secondaryButtonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowWeaponModificationButtons && expandButtons);
        buildButtonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowBuildWeaponButtons && expandButtons);

    }
}