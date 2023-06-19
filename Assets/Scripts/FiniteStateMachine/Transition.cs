using FiniteStateMachine.CreatureStateMachine;

namespace FiniteStateMachine {
    public class Transition<TState, TStateObject> where TState : State<TStateObject> where TStateObject : IAutomatable {
        public TState OriginState { get; private set; }
        public TState DestinationState { get; private set; }
        public bool CanInterrupts { get; private set; }

        private bool isTransitionPossible = true;

        public Transition(TState originState, TState destinationState, bool canInterrupts) {
            OriginState = originState;
            DestinationState = destinationState;
            CanInterrupts = canInterrupts;
        }

        public bool IsTransitionPossible() {
            return DestinationState.CanBeActivated();
        }
    }
}