using System;
using AmmoMagazines;
using UnityEngine;

namespace SecurityWeapons {
    public class SharedWeaponSpecifications : MonoBehaviour {
        public static SharedWeaponSpecifications Instance { get; private set; }

        [Header("Ground weapon")]
        [SerializeField] private int groundRequiredResources;

        [Tooltip("Min/Max angel that the weapon can rotate around y axis")]
        [SerializeField] private Vector2 groundRotateOnYAxisRange;
        public Vector3 GroundRotateOnYAxisRange => groundRotateOnYAxisRange;

        [Tooltip("Min/Max angel that the weapon can rotate around x axis")]
        [SerializeField] private Vector2 groundRotateOnXAxisRange;
        public Vector3 GroundRotateOnXAxisRange => groundRotateOnXAxisRange;

        [SerializeField] private float groundRange = 100;
        public float GroundRange => groundRange;

        [Header("Air weapon")]
        [SerializeField] private int airRequiredResources;

        [Tooltip("Min/Max angel that the weapon can rotate around y axis")]
        [SerializeField] private Vector2 airRotateOnYAxisRange;
        public Vector3 AirRotateOnYAxisRange => airRotateOnYAxisRange;

        [Tooltip("Min/Max angel that the weapon can rotate around x axis")]
        [SerializeField] private Vector2 airRotateOnXAxisRange;
        public Vector3 AirRotateOnXAxisRange => airRotateOnXAxisRange;

        [SerializeField] private float airRange = 150;
        public float AirRange => airRange;

        [Header("Fighter plane")]
        [SerializeField] private int fighterRequiredResources;

        [Tooltip("Min/Max angel that the weapon can rotate around y axis")]
        [SerializeField] private Vector2 fighterPlaneRotateOnYAxisRange;
        public Vector3 FighterPlaneRotateOnYAxisRange => fighterPlaneRotateOnYAxisRange;

        [Tooltip("Min/Max angel that the weapon can rotate around x axis")]
        [SerializeField] private Vector2 fighterPlaneRotateOnXAxisRange;
        public Vector3 FighterPlaneRotateOnXAxisRange => fighterPlaneRotateOnXAxisRange;

        [SerializeField] private float fighterPlaneRange = 150;
        public float FighterPlaneRange => fighterPlaneRange;

        [Header("Shared")]
        [SerializeField] private Color normalOutlineColor;
        public Color NormalOutlineColor => normalOutlineColor;

        [SerializeField] private Color selectionOutlineColor;
        public Color SelectionOutlineColor => selectionOutlineColor;

        [Tooltip("How much bullets to refill the magazine of the weapon on clicking the reload bullets button")]
        [SerializeField] private int bulletsAmountToReloadPerClick;

        [Tooltip("How much rockets to refill the magazine of the weapon on clicking the reload bullets button")]
        [SerializeField] private int rocketsAmountToReloadPerClick;

        [SerializeField] private int refundPercentOnSellingWeapon = 50;

        private void Awake() {
            if (Instance is not null) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public float GetWeaponRange(DefenceWeapon.WeaponsType weaponType) {
            return weaponType switch {
                DefenceWeapon.WeaponsType.Ground => groundRange,
                DefenceWeapon.WeaponsType.Air => airRange,
                DefenceWeapon.WeaponsType.FighterPlane => fighterPlaneRange,
                _ => throw new ArgumentOutOfRangeException(nameof(weaponType), weaponType, "No such weapon type")
            };
        }

        public Vector2 GetWeaponRotateOnYAxisRange(DefenceWeapon.WeaponsType weaponType) {
            return weaponType switch {
                DefenceWeapon.WeaponsType.Ground => groundRotateOnYAxisRange,
                DefenceWeapon.WeaponsType.Air => airRotateOnYAxisRange,
                DefenceWeapon.WeaponsType.FighterPlane => fighterPlaneRotateOnYAxisRange,
                _ => throw new ArgumentOutOfRangeException(nameof(weaponType), weaponType, "No such weapon type")
            };
        }

        public Vector2 GetWeaponRotateOnXAxisRange(DefenceWeapon.WeaponsType weaponType) {
            return weaponType switch {
                DefenceWeapon.WeaponsType.Ground => groundRotateOnXAxisRange,
                DefenceWeapon.WeaponsType.Air => airRotateOnXAxisRange,
                DefenceWeapon.WeaponsType.FighterPlane => fighterPlaneRotateOnXAxisRange,
                _ => throw new ArgumentOutOfRangeException(nameof(weaponType), weaponType, "No such weapon type")
            };
        }

        public int GetWeaponRequiredSupplies(DefenceWeapon.WeaponsType weaponType) {
            return weaponType switch {
                DefenceWeapon.WeaponsType.Ground => groundRequiredResources,
                DefenceWeapon.WeaponsType.Air => airRequiredResources,
                DefenceWeapon.WeaponsType.FighterPlane => fighterRequiredResources,
                _ => throw new ArgumentOutOfRangeException(nameof(weaponType), weaponType, "No such weapon type")
            };
        }

        public int GetAmmoRefillAmount(Magazine.AmmoType ammoType) {
            return ammoType switch {
                Magazine.AmmoType.Bullet => bulletsAmountToReloadPerClick,
                Magazine.AmmoType.Rocket => rocketsAmountToReloadPerClick,
                _ => throw new ArgumentOutOfRangeException(nameof(ammoType), ammoType, null)
            };
        }

        public int GetRefundAmountFromSellingWeapon(DefenceWeapon.WeaponsType weaponsType) {
            return (int) (refundPercentOnSellingWeapon / 100f * GetWeaponRequiredSupplies(weaponsType));
        }
    }
}