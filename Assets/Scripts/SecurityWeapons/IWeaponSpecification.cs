using UnityEngine;

namespace SecurityWeapons {
    public interface IWeaponSpecification {
        Vector3 RotateOnYAxisRange { get; }
        Vector3 RotateOnXAxisRange { get; }
    }
}