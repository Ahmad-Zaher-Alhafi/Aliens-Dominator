using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class ChasingTargetState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.ChasingTarget;
        public override bool CanBeActivated() => (AutomatedObject.IsPoisoned || !AutomatedObject.HasToFollowPath) && AutomatedObject.ObjectToAttack != null;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;
        

        public ChasingTargetState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Mover.ChaseTarget(OnMoverOrderFulfilled, AutomatedObject.ObjectToAttack.transform);
        }

        public override void Fulfil() {
            base.Fulfil();
            AutomatedObject.TargetReached = true;
        }
    }
}