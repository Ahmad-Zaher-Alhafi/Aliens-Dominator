using System;
using System.Collections.Generic;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Creature State Machine", fileName = "Creature State Machine")]
    public class CreatureStateMachineData : ScriptableObject {
        public List<StateData> statesData;

        [Serializable]
        public class StateData {
            public CreatureStateType originStateType;
            public List<TransitionData> transitionsData;

            [Serializable]
            public class TransitionData {
                public CreatureStateType destinationStateType;
                public bool canInterrupts;
            }
        }
    }
}