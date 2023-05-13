using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine.CreatureMachine;
using FiniteStateMachine.States;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FiniteStateMachine {
    public abstract class StateMachine<T> : MonoBehaviour where T : Object {
        protected Transform StatesHolder;

        public State CurrentState { get; private set; }

        protected readonly Dictionary<State, List<Transition>> StatesTransitions = new();

        protected readonly Dictionary<StateType, State> States = new();
        protected T ObjectToAutomate;
        protected List<StateVisualiser> StateVisualisers = new();

        private bool isInitialized;
        private List<TransitionVisualiser> transitionVisualisers;

        protected virtual void Awake() {
            transitionVisualisers = StatesHolder.GetComponentsInChildren<TransitionVisualiser>().ToList();
            StateVisualisers = StatesHolder.GetComponentsInChildren<StateVisualiser>().ToList();
        }

        private List<Transition> currentStatePossibleTransitions;

        public virtual void Init(T objectToAutomate, StateType initialState) {
            if (!isInitialized) {
                isInitialized = true;
                ObjectToAutomate = objectToAutomate;

                if (transitionVisualisers.Count == 0) return;

                CreateStates();

                LinkStatesWithTransitions();
            }

            CurrentState = States[initialState];
            CurrentState.Activate();
        }

        protected abstract void CreateStates();

        private void LinkStatesWithTransitions() {
            foreach (TransitionVisualiser transitionVisualiser in transitionVisualisers) {
                State originState = States[transitionVisualiser.OriginStateType];
                State destinationState = States[transitionVisualiser.DestinationStateType];;
                StatesTransitions[originState].Add(new Transition(transitionVisualiser, originState, destinationState));
            }
        }

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

        private void ActivateDestinationState(Transition transition) {
            if (transition.CanInterrupts) {
                CurrentState.Interrupt();
            }

            CurrentState = transition.DestinationState;
            CurrentState.Activate();
        }

        public T GetState<T>() where T : State {
            return States.Values.OfType<T>().Single();
        }
    }
}