using Creatures;
using UnityEditor;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class AirSecurityWeaponStateMachine :  SecurityWeaponStateMachine<FlyingCreature> {

#if UNITY_EDITOR
        [CustomEditor(typeof(AirSecurityWeaponStateMachine))]
        public class AirSecurityWeaponStateMachineEditor : SecurityWeaponStateMachineEditor { }
#endif
    }
}