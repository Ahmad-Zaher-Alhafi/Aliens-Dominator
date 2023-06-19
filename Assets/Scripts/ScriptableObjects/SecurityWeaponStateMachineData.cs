using System;
using System.Collections.Generic;
using FiniteStateMachine.SecurityWeaponMachine;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Security Weapon State Machine", fileName = "Security Weapon State Machine")]
    public class SecurityWeaponStateMachineData : ScriptableObject {
        public List<StateData> statesData;

        [Serializable]
        public class StateData {
            public SecurityWeaponStateType originStateType;
            public List<TransitionData> transitionsData;

            [Serializable]
            public class TransitionData {

                public SecurityWeaponStateType destinationStateType;
                public bool canInterrupts;
            }
        }
    }
}