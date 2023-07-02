using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine.CreatureStateMachine;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace FiniteStateMachine {
    public abstract class StateMachine<TState, TAutomatable, TType> : MonoBehaviour where TState : State<TAutomatable, TType> where TAutomatable : IAutomatable where TType : Enum {
        [SerializeField] private StateMachineData<TType> stateMachineData;
        protected StateMachineData<TType> StateMachineData => stateMachineData;
        public TState PrimaryState {
            get => primaryState;
            private set {
                primaryState = value;
                primaryState.IsActiveAsSecondaryState = false;
            }
        }
        private TState primaryState;

        protected readonly Dictionary<TState, List<Transition<TState, TAutomatable, TType>>> StatesTransitions = new();

        protected readonly Dictionary<Enum, TState> States = new();
        protected TAutomatable AutomatedObject;

        private bool isInitialized;

        private List<Transition<TState, TAutomatable, TType>> currentStatePossibleTransitions;

        public virtual void Init(TAutomatable automatedObject, Enum initialState) {
            if (!isInitialized) {
                isInitialized = true;
                AutomatedObject = automatedObject;

                CreateStates();
                LinkStatesWithTransitions();
                AssignSyncedWithStates();
                AssignInterruptStates();
            }

            PrimaryState = States[initialState];
            PrimaryState.Activate();
        }

        protected abstract void CreateStates();

        private void LinkStatesWithTransitions() {
            foreach (StateMachineData<TType>.StateData stateData in stateMachineData.statesData) {
                foreach (StateMachineData<TType>.StateData.TransitionData transitionData in stateData.transitionsData) {
                    TState originState = States[stateData.originStateType];
                    TState destinationState = States[transitionData.destinationStateType];
                    StatesTransitions[originState].Add(new Transition<TState, TAutomatable, TType>(originState, destinationState));
                }
            }
        }

        private void Update() => Tick();

        protected virtual void Tick() {
            foreach (TState state in States.Values) {
                if (state.IsActive) {
                    state.Tick();
                }
            }

            currentStatePossibleTransitions = StatesTransitions[PrimaryState].FindAll(transition => transition.IsTransitionPossible());

            // Find any transitions that can interrupt the PrimaryState
            foreach (var transition in currentStatePossibleTransitions.Where(transition => transition.DestinationState.CanInterruptState(PrimaryState))) {
                // If already active as a secondary state then mark it as primary state instead
                if (transition.DestinationState.IsActiveAsSecondaryState) {
                    Debug.Log($"{transition.DestinationState} state became a primary state");
                    PrimaryState = transition.DestinationState;
                    break;
                }

                ActivateDestinationState(transition);
                break;
            }

            // Find all states that can be synced with the PrimaryState
            foreach (TState state in States.Values) {
                if (state.IsActive) continue;
                if (!state.CanBeSyncedWith(PrimaryState)) continue;
                ActivateAsSecondaryState(state);
            }

            if (PrimaryState.IsActive) return;
            // Find the next primary state when the current PrimaryState is not active anymore
            foreach (var transition in currentStatePossibleTransitions) {
                ActivateDestinationState(transition);
                return;
            }
        }

        private void ActivateDestinationState(Transition<TState, TAutomatable, TType> transition) {
            PrimaryState = transition.DestinationState;
            InterruptStatesIfValid();
            PrimaryState.Activate();
        }

        /// <summary>
        /// All states that can be interrupted by the newly activated PrimaryState will be interrupted
        /// </summary>
        private void InterruptStatesIfValid() {
            foreach (TState state in States.Values) {
                if (state.IsActive && PrimaryState.CanInterruptState(state)) {
                    state.Interrupt();
                }
            }
        }

        private void ActivateAsSecondaryState(TState secondaryState) {
            secondaryState.Activate();
        }

        public T GetState<T>() where T : TState {
            return States.Values.OfType<T>().Single();
        }

        private void AssignSyncedWithStates() {
            List<State<TAutomatable, TType>> statesSyncedWith = new();
            Array enumValues = Enum.GetValues(typeof(TType));

            foreach (StateMachineData<TType>.StateData stateData in stateMachineData.statesData) {
                for (int i = 0; i < enumValues.Length; i++) {
                    int layer = 1 << i;
                    if (((int) (object) stateData.statesSyncedWithMask & layer) == 0) continue;
                    TType creatureStateType = (TType) Enum.ToObject(typeof(TType), i);
                    TState stateSyncedWith = States[creatureStateType];
                    statesSyncedWith.Add(stateSyncedWith);
                }

                States[stateData.originStateType].SetStatesSyncedWith(statesSyncedWith);
            }
        }

        private void AssignInterruptStates() {
            foreach (StateMachineData<TType>.StateData stateData in stateMachineData.statesData) {
                List<State<TAutomatable, TType>> interruptStates = new();
                Array enumValues = Enum.GetValues(typeof(TType));

                for (int i = 0; i < enumValues.Length; i++) {
                    int layer = 1 << i;
                    if (((int) (object) stateData.interruptStatesMask & layer) == 0) continue;
                    TType creatureStateType = (TType) Enum.ToObject(typeof(TType), i);
                    TState interruptState = States[creatureStateType];
                    interruptStates.Add(interruptState);
                }

                States[stateData.originStateType].SetInterruptStates(interruptStates);
            }
        }


#if UNITY_EDITOR
        private void ForceState(TType creatureStateType) {
            PrimaryState.Interrupt();
            PrimaryState = States[creatureStateType];
            PrimaryState.Activate();
        }

        private void ActivateSecondaryState(TType creatureStateType) {
            States[creatureStateType].Activate(true);
        }


        [CustomEditor(typeof(StateMachine<,,>))]
        public class StateMachineEditor<T> : Editor where T : TType {
            [SerializeField] private T primaryStateType;
            [SerializeField] private T secondaryStateType;

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                StateMachine<TState, TAutomatable, TType> stateMachine = (StateMachine<TState, TAutomatable, TType>) target;

                // Show current state
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current State");
                GUI.enabled = false;
                EditorGUILayout.TextField(stateMachine.PrimaryState?.ToString());
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // Force state popup
                EditorGUILayout.BeginHorizontal();
                primaryStateType = (T) EditorGUILayout.EnumPopup("State to force", primaryStateType);
                EditorGUILayout.EndHorizontal();

                // State to force button
                if (GUILayout.Button("Force state")) {
                    if (Application.isPlaying) {
                        stateMachine.ForceState(primaryStateType);
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }

                // Secondary State popup
                EditorGUILayout.BeginHorizontal();
                secondaryStateType = (T) EditorGUILayout.EnumPopup("Secondary State To Activate", secondaryStateType);
                EditorGUILayout.EndHorizontal();

                // Activate secondary State button
                if (GUILayout.Button("Activate secondary state")) {
                    if (Application.isPlaying) {
                        stateMachine.ActivateSecondaryState(secondaryStateType);
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}