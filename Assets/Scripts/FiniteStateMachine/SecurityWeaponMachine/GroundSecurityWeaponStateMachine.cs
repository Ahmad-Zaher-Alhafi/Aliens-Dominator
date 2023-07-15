using Creatures;
using UnityEditor;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GroundSecurityWeaponStateMachine :  SecurityWeaponStateMachine<GroundCreature> {

#if UNITY_EDITOR
        [CustomEditor(typeof(GroundSecurityWeaponStateMachine))]
        public class GroundSecurityWeaponStateMachineEditor : SecurityWeaponStateMachineEditor { }
#endif
    }
}