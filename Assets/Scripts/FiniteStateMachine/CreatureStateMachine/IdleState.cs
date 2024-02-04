using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class IdleState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Idle;
        public override bool CanBeActivated() => AutomatedObject.IsCinematic && IsNextCinematicState;
        public override float? Speed => 0;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => true;

        public IdleState(Creature creature, bool checkWhenAutomatingDisabled) : base(creature, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Mover.StayIdle(OnMoverOrderFulfilled);
            AutomatedObject.Animator.SetRandomIdleAnimation(OnAnimationFinished);
        }
    }
}