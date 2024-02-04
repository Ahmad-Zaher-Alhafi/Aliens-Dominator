using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class PatrollingState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Patrolling;
        public override bool CanBeActivated() => AutomatedObject.IsCinematic && IsNextCinematicState;
        public override float? Speed => AutomatedObject.PatrolSpeed;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public PatrollingState(Creature creature, bool checkWhenAutomatingDisabled) : base(creature, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Mover.Patrol(OnMoverOrderFulfilled);
        }
    }
}