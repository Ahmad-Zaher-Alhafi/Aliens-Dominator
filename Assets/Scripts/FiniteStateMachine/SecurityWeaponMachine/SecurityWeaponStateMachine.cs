using System;
using System.Collections.Generic;
using ScriptableObjects;
using SecurityWeapons;
using UnityEditor;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class SecurityWeaponStateMachine : StateMachine<SecurityWeaponState, SecurityWeapon, SecurityWeaponStateType> {
        public override void Init(SecurityWeapon automatedObject, Enum initialState) {
            base.Init(automatedObject, initialState);

            if (PrimaryState.Type is SecurityWeaponStateType.Shutdown) {
                PrimaryState.Fulfil();
            }
        }

        protected override void CreateStates() {
            foreach (SecurityWeaponStateMachineData.StateData stateData in StateMachineData.statesData) {
                SecurityWeaponState state = stateData.originStateType switch {
                    SecurityWeaponStateType.Shutdown => new ShutdownState(AutomatedObject),
                    SecurityWeaponStateType.Guarding => new GuardingState(AutomatedObject),
                    SecurityWeaponStateType.Aiming => new AimingState(AutomatedObject),
                    SecurityWeaponStateType.Shooting => new ShootingState(AutomatedObject),
                    SecurityWeaponStateType.Destroyed => new DestroyedState(AutomatedObject),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<SecurityWeaponState, SecurityWeapon, SecurityWeaponStateType>>());
                States.Add(state.Type, state);
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(SecurityWeaponStateMachine))]
        public class CreatureStateMachineEditor : StateMachineEditor<SecurityWeaponStateType> { }
#endif
    }
}