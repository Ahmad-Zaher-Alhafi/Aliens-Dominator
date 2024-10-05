using Placeables;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Extensions;

public class WeaponConstructionPanel : MonoBehaviour, IPlaceableObject, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private GameObject buttonsHolder;
    [SerializeField] private GameObject secondaryButtonsPatent;
    [SerializeField] private GameObject buildButton;
    [SerializeField] private GameObject repairButton;
    public GameObject GameObject => gameObject;

    private WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable;
    private bool areSecondaryButtonsShown;

    private void Awake() {
        buttonsHolder.SetActive(false);
    }

    public void SetPlaceable(AddressablePlaceable placeable) {
        weaponConstructionPanelPlaceable = (WeaponConstructionPanelPlaceable) placeable;
    }

    private void Update() {
        buttonsHolder.SetActiveWithCheck(weaponConstructionPanelPlaceable.IsVisible);
    }

    public void BuildButtonClicked() {
        if (weaponConstructionPanelPlaceable.IsBuilt) return;

        buildButton.SetActive(false);
        repairButton.SetActive(true);

        weaponConstructionPanelPlaceable.BuildWeapon();

        OnPointerEnter(null);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!weaponConstructionPanelPlaceable.IsBuilt) return;

        areSecondaryButtonsShown = true;
        secondaryButtonsPatent.SetActive(areSecondaryButtonsShown);
        weaponConstructionPanelPlaceable.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!weaponConstructionPanelPlaceable.IsBuilt) return;

        areSecondaryButtonsShown = false;
        secondaryButtonsPatent.SetActive(areSecondaryButtonsShown);
        weaponConstructionPanelPlaceable.OnPointerExit();
    }
}