using System;
using System.Collections.Generic;
using ScriptableObjects;
using SecurityWeapons;
using UnityEditor;
using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class SecurityWeaponStateMachine : StateMachine<SecurityWeaponState, SecurityWeapon> {
        [SerializeField] private SecurityWeaponStateMachineData securityWeaponStateMachineData;
        public override SecurityWeaponState CurrentState { get; protected set; }

        public override void Init(SecurityWeapon objectToAutomate, Enum initialState) {
            base.Init(objectToAutomate, initialState);

            if (CurrentState.Type is SecurityWeaponStateType.Shutdown) {
                CurrentState.Fulfil();
            }
        }

        protected override void CreateStates() {
            foreach (SecurityWeaponStateMachineData.StateData stateData in securityWeaponStateMachineData.statesData) {
                SecurityWeaponState state = stateData.originStateType switch {
                    SecurityWeaponStateType.Shutdown => new ShutdownState(ObjectToAutomate),
                    SecurityWeaponStateType.Guarding => new GuardingState(ObjectToAutomate),
                    SecurityWeaponStateType.Aiming => new AimingState(ObjectToAutomate),
                    SecurityWeaponStateType.Shooting => new ShootingState(ObjectToAutomate),
                    SecurityWeaponStateType.Destroyed => new DestroyedState(ObjectToAutomate),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<SecurityWeaponState, SecurityWeapon>>());
                States.Add(state.Type, state);
            }
        }

        protected override void LinkStatesWithTransitions() {
            foreach (SecurityWeaponStateMachineData.StateData stateData in securityWeaponStateMachineData.statesData) {
                foreach (SecurityWeaponStateMachineData.StateData.TransitionData transitionData in stateData.transitionsData) {
                    SecurityWeaponState originState = States[stateData.originStateType];
                    SecurityWeaponState destinationState = States[transitionData.destinationStateType];
                    StatesTransitions[originState].Add(new Transition<SecurityWeaponState, SecurityWeapon>(originState, destinationState, transitionData.canInterrupts));
                }
            }
        }

        protected override void Tick() {
            if (ObjectToAutomate.IsDestroyed) return;
            base.Tick();
        }


        private void ForceState(SecurityWeaponStateType creatureStateType) {
            CurrentState.Interrupt();
            CurrentState = States[creatureStateType];
            CurrentState.Activate();
        }

        
#if UNITY_EDITOR
        [CustomEditor(typeof(SecurityWeaponStateMachine))]
        public class SecurityWeaponStateEditor : Editor {
            [SerializeField] private SecurityWeaponStateType securityWeaponStateType;
            [SerializeField] private bool overrideCurrentState;

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                SecurityWeaponStateMachine securityWeaponStateMachine = (SecurityWeaponStateMachine) target;

                // Show current state
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current State");
                GUI.enabled = false;
                EditorGUILayout.TextField(securityWeaponStateMachine.CurrentState?.Type.ToString() ?? SecurityWeaponStateType.Shutdown.ToString());
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                overrideCurrentState = EditorGUILayout.Toggle("Override Current State", overrideCurrentState);
                EditorGUILayout.EndHorizontal();

                if (!overrideCurrentState) return;

                EditorGUILayout.BeginHorizontal();
                securityWeaponStateType = (SecurityWeaponStateType) EditorGUILayout.EnumPopup("State to force", securityWeaponStateType);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Force state")) {
                    if (Application.isPlaying) {
                        securityWeaponStateMachine.ForceState(securityWeaponStateType);
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}