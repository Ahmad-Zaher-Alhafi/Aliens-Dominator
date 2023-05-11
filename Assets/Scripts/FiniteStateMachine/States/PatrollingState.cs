using Creatures;

namespace FiniteStateMachine.States {
    public class PatrollingState : State {
        public override StateType Type => StateType.Patrolling;
        public override bool CanBeActivated() => Creature.IsCinematic && IsNextCinematicState;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public PatrollingState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();
            Creature.Mover.Patrol(OnMoverOrderFulfilled);
        }

        public override void Interrupt() {
            base.Interrupt();
            Creature.Mover.TerminateCurrentOrder();
        }
    }
}