using Creatures;
using UnityEditor;

namespace SecurityWeapons {
    public class AirWeaponSensor : WeaponSensor<FlyingCreature> { }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(AirWeaponSensor))]
    public class AirWeaponSensorEditor : WeaponSensor<FlyingCreature>.WeaponSensorEditor { }
#endif
}