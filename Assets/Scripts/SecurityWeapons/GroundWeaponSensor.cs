using Creatures;
using UnityEditor;

namespace SecurityWeapons {
    public class GroundWeaponSensor : WeaponSensor<GroundCreature> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(GroundWeaponSensor))]
    public class GroundWeaponSensorEditor : WeaponSensor<GroundCreature>.WeaponSensorEditor { }
#endif
}