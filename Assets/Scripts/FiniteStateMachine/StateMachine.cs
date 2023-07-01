using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine.CreatureStateMachine;
using ScriptableObjects;
using UnityEngine;

namespace FiniteStateMachine {
    public abstract class StateMachine<TState, TStateObject, TEnum> : MonoBehaviour where TState : State<TStateObject> where TStateObject : IAutomatable where TEnum : Enum {
        [SerializeField] private StateMachineData<TEnum> stateMachineData;
        protected StateMachineData<TEnum> StateMachineData => stateMachineData;
        public abstract TState PrimaryState { get; protected set; }

        protected readonly Dictionary<TState, List<Transition<TState, TStateObject>>> StatesTransitions = new();

        protected readonly Dictionary<Enum, TState> States = new();
        protected TStateObject ObjectToAutomate;

        private bool isInitialized;

        private List<Transition<TState, TStateObject>> currentStatePossibleTransitions;
        /// <summary>
        /// Secondary states are the states that got activated because they can be synced with the current state
        /// These states get terminated once they are fulfilled, they do not translate to other states
        /// </summary>
        private List<TState> secondaryStates = new();

        public virtual void Init(TStateObject objectToAutomate, Enum initialState) {
            if (!isInitialized) {
                isInitialized = true;
                ObjectToAutomate = objectToAutomate;

                CreateStates();
                LinkStatesWithTransitions();
                AssignSyncedWithStates();
            }

            PrimaryState = States[initialState];
            PrimaryState.Activate();
        }

        protected abstract void CreateStates();

        private void LinkStatesWithTransitions() {
            foreach (StateMachineData<TEnum>.StateData stateData in stateMachineData.statesData) {
                foreach (StateMachineData<TEnum>.StateData.TransitionData transitionData in stateData.transitionsData) {
                    TState originState = States[stateData.originStateType];
                    TState destinationState = States[transitionData.destinationStateType];
                    StatesTransitions[originState].Add(new Transition<TState, TStateObject>(originState, destinationState, transitionData.canInterrupts));
                }
            }
        }

        private void Update() => Tick();

        protected virtual void Tick() {
            if (PrimaryState.IsActive) {
                PrimaryState.Tick();
            }

            foreach (TState secondaryState in secondaryStates) {
                if (secondaryState.IsActive) {
                    secondaryState.Tick();
                }
            }

            currentStatePossibleTransitions = StatesTransitions[PrimaryState].FindAll(transition => transition.IsTransitionPossible());

            // Find any transitions that can interrupt the current state
            foreach (var transition in currentStatePossibleTransitions.Where(transition => transition.CanInterrupts)) {
                ActivateDestinationState(transition);
                return;
            }

            // Find any states that can be synced with the current state
            foreach (var transition in currentStatePossibleTransitions.Where(transition => PrimaryState.IsSyncedWith(transition.DestinationState))) {
                ActivateSecondaryState(transition.DestinationState);
                return;
            }

            if (PrimaryState.IsActive) return;
            // Find the next state when the currentState is not active anymore
            foreach (var transition in currentStatePossibleTransitions) {
                ActivateDestinationState(transition);
                return;
            }
        }

        private void ActivateDestinationState(Transition<TState, TStateObject> transition) {
            if (transition.CanInterrupts) {
                PrimaryState.Interrupt();
            }

            PrimaryState = transition.DestinationState;
            PrimaryState.Activate();
        }

        private void ActivateSecondaryState(TState secondaryState) {
            secondaryStates.Add(secondaryState);
            secondaryState.Activate();
        }

        public T GetState<T>() where T : TState {
            return States.Values.OfType<T>().Single();
        }

        private void AssignSyncedWithStates() {
            List<State<TStateObject>> statesSyncedWith = new();
            Array enumValues = Enum.GetValues(typeof(TEnum));

            foreach (StateMachineData<TEnum>.StateData stateData in stateMachineData.statesData) {
                for (int i = 0; i < enumValues.Length; i++) {
                    int layer = 1 << i;
                    if (((int) (object) stateData.statesSyncedWith & layer) == 0) continue;
                    TEnum creatureStateType = (TEnum) Enum.ToObject(typeof(TEnum), i);
                    TState stateSyncedWith = States[creatureStateType];
                    statesSyncedWith.Add(stateSyncedWith);
                }

                States[stateData.originStateType].SetStatesSyncedWith(statesSyncedWith);
            }
        }
    }
}