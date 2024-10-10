﻿using System.Collections.Generic;
using System.Linq;
using AmmoMagazines;
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
        public int BulletsAmountInMagazine => weaponConstructionPoint.IsWeaponBuilt ? weaponConstructionPoint.BuiltWeapon.GetProjectileAmountInMagazine() : 0;
        public int RocketsAmountInMagazine => weaponConstructionPoint.IsWeaponBuilt ? weaponConstructionPoint.BuiltWeapon.GetProjectileAmountInMagazine(Magazine.AmmoType.Rocket) : 0;

        private readonly WeaponConstructionPoint weaponConstructionPoint;

        public WeaponConstructionPanelPlaceable(WeaponConstructionPoint weaponConstructionPoint) : base("Assets/Prefabs/Weapon Construction Panel.prefab") {
            this.weaponConstructionPoint = weaponConstructionPoint;
        }

        public bool TryBuildWeapon(DefenceWeapon.WeaponsType weaponType) {
            return Ctx.Deps.ConstructionController.TryBuildWeapon(weaponType, weaponConstructionPoint);
        }

        public bool HasEnoughSuppliesToBuildWeapon(DefenceWeapon.WeaponsType weaponType) {
            return Ctx.Deps.SuppliesController.HasEnoughSupplies(SuppliesController.SuppliesTypes.Construction, SharedWeaponSpecifications.Instance.GetWeaponRequiredSupplies(weaponType));
        }

        public void ShowWeaponPlaceholder(DefenceWeapon.WeaponsType weaponType) {
            WeaponPlaceholder.Instance.ShowPlaceholder(weaponType, weaponConstructionPoint.WeaponCreatePosition, weaponConstructionPoint.Rotation);
        }

        public void RefillAmmo(Magazine.AmmoType ammoType) {
            weaponConstructionPoint.BuiltWeapon.Reload(SharedWeaponSpecifications.Instance.GetAmmoRefillAmount(ammoType));
        }

        public void HideWeaponPlaceholder() {
            WeaponPlaceholder.Instance.HidePlaceholder();
        }

        public void BulldozeWeapon() { }

        public void RepairWeapon() { }

        public void UpgradeWeapon() { }

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