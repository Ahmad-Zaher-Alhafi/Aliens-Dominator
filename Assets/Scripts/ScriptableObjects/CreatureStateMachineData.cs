using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Creature State Machine", fileName = "Creature State Machine")]
    public class CreatureStateMachineData : StateMachineData<CreatureStateType> { }
}