using FiniteStateMachine.SecurityWeaponMachine;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Security Weapon State Machine", fileName = "Security Weapon State Machine")]
    public class SecurityWeaponStateMachineData : StateMachineData<SecurityWeaponStateType> { }
}