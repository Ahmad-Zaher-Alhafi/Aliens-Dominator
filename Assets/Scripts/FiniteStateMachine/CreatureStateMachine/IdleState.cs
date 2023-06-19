using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class IdleState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Idle;
        public override bool CanBeActivated() => StateObject.IsCinematic && IsNextCinematicState;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public IdleState(Creature creature) : base(creature) { }

        public override void Activate() {
            base.Activate();
            StateObject.Mover.StayIdle(OnMoverOrderFulfilled);
        }
    }
}