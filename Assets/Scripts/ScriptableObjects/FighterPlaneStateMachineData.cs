using FiniteStateMachine.FighterPlaneStateMachine;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Fighter Plane State Machine", fileName = "Fighter Plane State Machine")]
    public class FighterPlaneStateMachineData : StateMachineData<FighterPlaneStateType> { }
}