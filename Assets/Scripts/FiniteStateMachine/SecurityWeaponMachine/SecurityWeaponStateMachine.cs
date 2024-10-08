using System;
using System.Collections.Generic;
using ScriptableObjects;
using SecurityWeapons;
using UnityEditor;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public abstract class SecurityWeaponStateMachine<TEnemyType> : StateMachine<SecurityWeaponState<TEnemyType>, SecurityWeapon<TEnemyType>, SecurityWeaponStateType> where TEnemyType : IAutomatable {
        public override void Init(SecurityWeapon<TEnemyType> automatedObject, Enum initialState) {
            base.Init(automatedObject, initialState);
            GetState<ShutdownState<TEnemyType>>().Fulfil();
        }

        protected override void CreateStates() {
            foreach (SecurityWeaponStateMachineData.StateData stateData in StateMachineData.statesData) {
                SecurityWeaponState<TEnemyType> state = stateData.originStateType switch {
                    SecurityWeaponStateType.Shutdown => new ShutdownState<TEnemyType>(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    SecurityWeaponStateType.Guarding => new GuardingState<TEnemyType>(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    SecurityWeaponStateType.Aiming => new AimingState<TEnemyType>(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    SecurityWeaponStateType.Shooting => new ShootingState<TEnemyType>(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    SecurityWeaponStateType.Destroyed => new DestroyedState<TEnemyType>(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<SecurityWeaponState<TEnemyType>, SecurityWeapon<TEnemyType>, SecurityWeaponStateType>>());
                States.Add(state.Type, state);
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(SecurityWeaponStateMachine<>))]
        public class SecurityWeaponStateMachineEditor : StateMachineEditor<SecurityWeaponStateType> { }
#endif
    }
}