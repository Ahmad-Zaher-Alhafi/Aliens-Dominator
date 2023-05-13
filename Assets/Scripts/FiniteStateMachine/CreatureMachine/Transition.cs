using FiniteStateMachine.States;

namespace FiniteStateMachine.CreatureMachine {
    public class Transition {
        public State OriginState { get; private set; }
        public State DestinationState { get; private set; }
        public bool CanInterrupts { get; private set; }

        private TransitionVisualiser transitionVisualiser;
        private bool isTransitionPossible = true;

        public Transition(TransitionVisualiser transitionVisualiser, State originState, State destinationState) {
            this.transitionVisualiser = transitionVisualiser;
            OriginState = originState;
            DestinationState = destinationState;
            CanInterrupts = transitionVisualiser.CanInterrupts;
        }

        public bool IsTransitionPossible() {
            return DestinationState.CanBeActivated();
        }
    }
}