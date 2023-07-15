using UnityEngine;

namespace SecurityWeapons {
    public interface IWeaponSpecification {
        Vector3 RotateXRange { get; }
        Vector3 RotateYRange { get; }
        float AimingSpeed { get; }
        float BulletsPerSecond { get; }
    }
}