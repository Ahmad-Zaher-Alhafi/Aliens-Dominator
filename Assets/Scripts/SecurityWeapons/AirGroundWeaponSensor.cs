using Creatures;
using UnityEditor;

namespace SecurityWeapons {
    public class AirGroundWeaponSensor : WeaponSensor<Creature> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(AirGroundWeaponSensor))]
    public class AirGroundWeaponSensorEditor : WeaponSensor<Creature>.WeaponSensorEditor { }
#endif
}