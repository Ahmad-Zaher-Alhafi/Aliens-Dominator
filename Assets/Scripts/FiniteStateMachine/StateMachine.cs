using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;

namespace FiniteStateMachine {
    public abstract class StateMachine<TState, TStateObject> : MonoBehaviour where TState : State<TStateObject> where TStateObject : IAutomatable {
        public abstract TState CurrentState { get; protected set; }

        protected readonly Dictionary<TState, List<Transition<TState, TStateObject>>> StatesTransitions = new();

        protected readonly Dictionary<Enum, TState> States = new();
        protected TStateObject ObjectToAutomate;

        private bool isInitialized;

        private List<Transition<TState, TStateObject>> currentStatePossibleTransitions;

        public virtual void Init(TStateObject objectToAutomate, Enum initialState) {
            if (!isInitialized) {
                isInitialized = true;
                ObjectToAutomate = objectToAutomate;

                CreateStates();
                LinkStatesWithTransitions();
            }

            CurrentState = States[initialState];
            CurrentState.Activate();
        }

        protected abstract void CreateStates();

        protected abstract void LinkStatesWithTransitions();

        private void Update() => Tick();

        protected virtual void Tick() {
            if (CurrentState.IsActive) {
                CurrentState.Tick();
            }

            currentStatePossibleTransitions = StatesTransitions[CurrentState].FindAll(transition => transition.IsTransitionPossible());

            // Find any transitions that can interrupt the current state
            foreach (var transition in currentStatePossibleTransitions.Where(transition => transition.CanInterrupts)) {
                ActivateDestinationState(transition);
                return;
            }

            if (CurrentState.IsActive) return;
            // Find the next state when the currentState is not active anymore
            foreach (var transition in currentStatePossibleTransitions) {
                ActivateDestinationState(transition);
                return;
            }
        }

        private void ActivateDestinationState(Transition<TState, TStateObject> transition) {
            if (transition.CanInterrupts) {
                CurrentState.Interrupt();
            }

            CurrentState = transition.DestinationState;
            CurrentState.Activate();
        }

        public T GetState<T>() where T : TState {
            return States.Values.OfType<T>().Single();
        }
    }
}