using System.Collections.Generic;
using System.Linq;
using Context;
using ManagersAndControllers;
using SecurityWeapons;

namespace Placeables {
    public class WeaponConstructionPanelPlaceable : AddressablePlaceable {
        public bool IsVisible => Ctx.Deps.GameController.CurrentViewMode == GameController.ViewMode.TopDown && !weaponConstructionPoint.IsWeaponBuilt &&
                                 !Ctx.Deps.CameraController.IsBlending && !isHiddenForMoreSpace;
        public bool IsBuilt { get; private set; }
        public IReadOnlyList<DefenceWeapon.WeaponType> WeaponTypesToShow => weaponConstructionPoint.WeaponTypesThatCanBeBuiltInThisPoint;

        private bool isHiddenForMoreSpace;
        private readonly WeaponConstructionPoint weaponConstructionPoint;

        public WeaponConstructionPanelPlaceable(WeaponConstructionPoint weaponConstructionPoint) : base("Assets/Prefabs/Weapon Construction Panel.prefab") {
            this.weaponConstructionPoint = weaponConstructionPoint;
        }

        public void BuildWeapon(DefenceWeapon.WeaponType weaponType) {
            IsBuilt = true;
            Ctx.Deps.ConstructionController.BuildWeapon(weaponType, weaponConstructionPoint);
        }

        public void BulldozeWeapon() { }

        public void RepairWeapon() { }

        public void UpgradeWeapon() { }

        public void RefillAmmo() { }

        public void OnWeaponDestroyed() { }

        public void OnPointerEnter() {
            HideAllOtherPanelsForSpace();
        }
        public void OnPointerExit() {
            ShowAllOtherPanels();
        }

        private void HideAllOtherPanelsForSpace() {
            foreach (WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable in Ctx.Deps.PlaceablesController.GetPlaceablesOfType<WeaponConstructionPanelPlaceable>().Where(placeable => placeable != this)) {
                weaponConstructionPanelPlaceable.isHiddenForMoreSpace = true;
            }
        }

        private void ShowAllOtherPanels() {
            foreach (WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable in Ctx.Deps.PlaceablesController.GetPlaceablesOfType<WeaponConstructionPanelPlaceable>().Where(placeable => placeable != this)) {
                weaponConstructionPanelPlaceable.isHiddenForMoreSpace = false;
            }
        }
    }
}