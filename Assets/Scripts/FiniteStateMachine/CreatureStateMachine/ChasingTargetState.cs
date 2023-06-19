using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class ChasingTargetState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.ChasingTarget;
        public override bool CanBeActivated() => (StateObject.IsPoisoned || !StateObject.HasToFollowPath) && StateObject.ObjectToAttack != null;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;
        

        public ChasingTargetState(Creature creature) : base(creature) { }

        public override void Activate() {
            base.Activate();
            StateObject.Mover.ChaseTarget(OnMoverOrderFulfilled, StateObject.ObjectToAttack.transform.position);
        }

        public override void Fulfil() {
            base.Fulfil();
            StateObject.TargetReached = true;
        }
    }
}