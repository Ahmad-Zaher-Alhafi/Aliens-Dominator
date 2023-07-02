using System;
using FiniteStateMachine.CreatureStateMachine;

namespace FiniteStateMachine {
    public class Transition<TState, TAutomatable, TType> where TState : State<TAutomatable, TType> where TAutomatable : IAutomatable where TType : Enum {
        public TState OriginState { get; private set; }
        public TState DestinationState { get; private set; }

        private bool isTransitionPossible = true;

        public Transition(TState originState, TState destinationState) {
            OriginState = originState;
            DestinationState = destinationState;
        }

        public bool IsTransitionPossible() {
            return DestinationState.CanBeActivated();
        }
    }
}