using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class FollowingPathState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.FollowingPath;
        public override bool CanBeActivated() => AutomatedObject.HasToFollowPath;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public FollowingPathState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Mover.FollowPath(OnMoverOrderFulfilled);
        }

        public override void Fulfil() {
            base.Fulfil();
            AutomatedObject.HasToFollowPath = false;
        }
    }
}