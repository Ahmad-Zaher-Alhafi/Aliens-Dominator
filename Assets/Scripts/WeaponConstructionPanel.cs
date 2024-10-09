using System.Linq;
using DG.Tweening;
using Placeables;
using SecurityWeapons;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Extensions;

public class WeaponConstructionPanel : MonoBehaviour, IPlaceableObject, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private GameObject buttonsHolder;

    [Header("Secondary buttons")]
    [SerializeField] private GameObject repairButton;
    [SerializeField] private GameObject secondaryButtonsHolder;

    [Header("Build buttons and price texts")]
    [SerializeField] private GameObject buildButtonsHolder;

    [SerializeField] private GameObject groundBuildWeaponButton;
    [SerializeField] private TextMeshProUGUI groundBuildWeaponPriceText;

    [SerializeField] private GameObject airBuildWeaponButton;
    [SerializeField] private TextMeshProUGUI airBuildWeaponPriceText;

    [SerializeField] private GameObject fighterBuildPlaneWeaponButton;
    [SerializeField] private TextMeshProUGUI fighterBuildWeaponPriceText;

    public GameObject GameObject => gameObject;

    private WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable;
    private bool expandButtons;
    private CanvasGroup buttonsHolderCanvasGroup;
    private Sequence textShakeTween;

    private void Awake() {
        buttonsHolder.SetActive(false);
        buttonsHolderCanvasGroup = buttonsHolder.GetComponent<CanvasGroup>();

        groundBuildWeaponPriceText.text = SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType.Ground).ToString();
        airBuildWeaponPriceText.text = SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType.Air).ToString();
        fighterBuildWeaponPriceText.text = SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType.FighterPlane).ToString();
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
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.Ground)) {
            textShakeTween?.Complete();
            textShakeTween = DOTween.Sequence()
                .Join(groundBuildWeaponPriceText.transform.DOShakePosition(.5f, 5))
                .Join(groundBuildWeaponPriceText.DOColor(Colors.Instance.Error, .5f))
                .Append(groundBuildWeaponPriceText.DOColor(Colors.Instance.Normal, .25f))
                .Play();

            return;
        }

        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.Ground);
        OnPointerEnter(null);
    }

    public void BuildAirWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.Air)) {
            textShakeTween?.Complete();
            textShakeTween = DOTween.Sequence()
                .Join(airBuildWeaponPriceText.transform.DOShakePosition(.5f, 5))
                .Join(airBuildWeaponPriceText.DOColor(Colors.Instance.Error, .25f))
                .Append(airBuildWeaponPriceText.DOColor(Colors.Instance.Normal, .25f))
                .Play();

            return;
        }

        weaponConstructionPanelPlaceable.BuildWeapon(DefenceWeapon.WeaponsType.Air);
        OnPointerEnter(null);
        OnWeaponButtonPointerExit();
    }

    public void BuildFighterPlaneWeaponButtonClicked() {
        if (!weaponConstructionPanelPlaceable.HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType.FighterPlane)) {
            textShakeTween?.Complete();
            textShakeTween = DOTween.Sequence()
                .Join(fighterBuildWeaponPriceText.transform.DOShakePosition(.5f, 5))
                .Join(fighterBuildWeaponPriceText.DOColor(Colors.Instance.Error, .5f))
                .Append(fighterBuildWeaponPriceText.DOColor(Colors.Instance.Normal, .25f))
                .Play();

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

        groundBuildWeaponButton.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowBuildWeaponButtons);
        repairButton.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowWeaponModificationButtons);

        secondaryButtonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowWeaponModificationButtons && expandButtons);
        buildButtonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.ShowBuildWeaponButtons && expandButtons);

    }
}