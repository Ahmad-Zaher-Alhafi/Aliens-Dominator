using UnityEngine;

namespace SecurityWeapons {
    public interface IWeaponSpecification {
        Transform Transform { get; }
        Vector3 RotateOnYAxisRange { get; }
        Vector3 RotateOnXAxisRange { get; }
        float AimingSpeed { get; }
        float BulletsPerSecond { get; }
    }
}