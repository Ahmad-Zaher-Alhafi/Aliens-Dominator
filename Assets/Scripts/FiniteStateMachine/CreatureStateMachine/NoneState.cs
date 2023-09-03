using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class NoneState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.None;
        public override bool CanBeActivated() => AutomatedObject.IsStateActive<NoneState>();
        public override float? Speed => 0;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => false;

        public NoneState(Creature creature) : base(creature) { }
    }
}