using Creatures;

namespace FiniteStateMachine.States {
    public class FollowingPathState : State {
        public override StateType Type => StateType.FollowingPath;
        public override bool CanBeActivated() => Creature.HasToFollowPath;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public FollowingPathState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();
            Creature.Mover.FollowPath(OnMoverOrderFulfilled);
        }

        public override void Fulfil() {
            base.Fulfil();
            Creature.HasToFollowPath = false;
        }
    }
}