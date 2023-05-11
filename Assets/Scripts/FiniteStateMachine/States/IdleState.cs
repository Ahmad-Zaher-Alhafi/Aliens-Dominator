using Creatures;

namespace FiniteStateMachine.States {
    public class IdleState : State {
        public override StateType Type => StateType.Idle;
        public override bool CanBeActivated() => Creature.IsCinematic && IsNextCinematicState;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public IdleState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();
            Creature.Mover.StayIdle(OnMoverOrderFulfilled);
            Creature.Animator.PlayIdleAnimation(OnAnimationFinished);
        }
    }
}