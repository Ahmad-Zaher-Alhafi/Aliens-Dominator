using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class PatrollingState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Patrolling;
        public override bool CanBeActivated() => StateObject.IsCinematic && IsNextCinematicState;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public PatrollingState(Creature creature) : base(creature) { }

        public override void Activate() {
            base.Activate();
            StateObject.Mover.Patrol(OnMoverOrderFulfilled);
        }
    }
}