using System.Collections.Generic;
using System.Linq;
using Context;
using ManagersAndControllers;
using SecurityWeapons;
using UnityEngine;

namespace Placeables {
    public class WeaponConstructionPanelPlaceable : AddressablePlaceable {
        public bool IsVisible => Ctx.Deps.GameController.CurrentViewMode == GameController.ViewMode.TopDown && !Ctx.Deps.CameraController.IsBlending;
        public bool ShowBuildWeaponButtons => !weaponConstructionPoint.IsWeaponBuilt;
        public bool ShowWeaponModificationButtons => weaponConstructionPoint.IsWeaponBuilt;
        public IReadOnlyList<DefenceWeapon.WeaponsType> WeaponTypesToShow => weaponConstructionPoint.WeaponTypesThatCanBeBuiltInThisPoint;
        public Vector3 Position => Ctx.Deps.CameraController.LocalActiveCamera.WorldToScreenPoint(weaponConstructionPoint.WeaponCreatePosition) + Vector3.up * 80;
        /// <summary>
        /// True when some other panel is hovered
        /// </summary>
        public bool IsDimmed { get; private set; }

        private readonly WeaponConstructionPoint weaponConstructionPoint;

        public WeaponConstructionPanelPlaceable(WeaponConstructionPoint weaponConstructionPoint) : base("Assets/Prefabs/Weapon Construction Panel.prefab") {
            this.weaponConstructionPoint = weaponConstructionPoint;
        }

        public void BuildWeapon(DefenceWeapon.WeaponsType weaponType) {
            Ctx.Deps.ConstructionController.BuildWeapon(weaponType, weaponConstructionPoint);
        }

        public bool HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType weaponType) {
            return Ctx.Deps.SuppliesController.HasEnoughSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(weaponType));
        }

        public void ShowWeaponPlaceholder(DefenceWeapon.WeaponsType weaponType) {
            WeaponPlaceholder.Instance.ShowPlaceholder(weaponType, weaponConstructionPoint.WeaponCreatePosition, weaponConstructionPoint.Rotation);
        }

        public void HideWeaponPlaceholder() {
            WeaponPlaceholder.Instance.HidePlaceholder();
        }

        public void BulldozeWeapon() { }

        public void RepairWeapon() { }

        public void UpgradeWeapon() { }

        public void RefillAmmo() { }

        public void OnWeaponDestroyed() { }

        public void OnPointerEnter() {
            HideAllOtherPanelsForSpace();
            weaponConstructionPoint.OnSelected();
        }
        public void OnPointerExit() {
            ShowAllOtherPanels();
            weaponConstructionPoint.OnDeselected();
        }

        private void HideAllOtherPanelsForSpace() {
            foreach (WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable in Ctx.Deps.PlaceablesController.GetPlaceablesOfType<WeaponConstructionPanelPlaceable>().Where(placeable => placeable != this)) {
                weaponConstructionPanelPlaceable.IsDimmed = true;
            }
        }

        private void ShowAllOtherPanels() {
            foreach (WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable in Ctx.Deps.PlaceablesController.GetPlaceablesOfType<WeaponConstructionPanelPlaceable>().Where(placeable => placeable != this)) {
                weaponConstructionPanelPlaceable.IsDimmed = false;
            }
        }
    }
}