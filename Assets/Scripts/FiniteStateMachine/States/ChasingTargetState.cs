using Creatures;

namespace FiniteStateMachine.States {
    public class ChasingTargetState : State {
        public override StateType Type => StateType.ChasingTarget;
        public override bool CanBeActivated() => (Creature.IsPoisoned || !Creature.HasToFollowPath) && Creature.ObjectToAttack != null;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;
        

        public ChasingTargetState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();
            Creature.Mover.ChaseTarget(OnMoverOrderFulfilled, Creature.ObjectToAttack.transform.position);
        }

        public override void Fulfil() {
            base.Fulfil();
            Creature.TargetReached = true;
        }
    }
}