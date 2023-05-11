using Creatures;

namespace FiniteStateMachine.States {
    public class NoneState : State {
        public override StateType Type => StateType.None;
        public override bool CanBeActivated() => Creature.CurrentState == StateType.None;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => false;
        
        public NoneState(bool isFinal, Creature creature) : base(isFinal, creature) { }
    }
}