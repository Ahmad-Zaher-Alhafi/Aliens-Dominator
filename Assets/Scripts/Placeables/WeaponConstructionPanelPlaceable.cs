using System.Linq;
using Context;
using ManagersAndControllers;

namespace Placeables {
    public class WeaponConstructionPanelPlaceable : AddressablePlaceable {
        public bool IsVisible => Ctx.Deps.GameController.CurrentViewMode == GameController.ViewMode.TopDown && !Ctx.Deps.CameraController.IsBlending && !isHiddenForMoreSpace;
        public bool IsBuilt { get; private set; }

        private bool isHiddenForMoreSpace;

        public WeaponConstructionPanelPlaceable() : base("Assets/Prefabs/Weapon Construction Panel.prefab") { }

        public void BuildWeapon() {
            IsBuilt = true;
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