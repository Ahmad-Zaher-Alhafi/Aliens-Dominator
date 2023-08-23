using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public enum CreatureStateType {
        None,
        Idle,
        Patrolling,
        FollowingPath,
        ChasingTarget,
        GettingHit,
        Attacking,
        RunningAway,
        Dead,
        SpecialAbility,
    }

    public abstract class CreatureState : State<Creature, CreatureStateType> {
        public virtual bool IsCinematic => false;

        public bool IsNextCinematicState;

        protected abstract bool WaitForMoverToFulfill { get; }
        protected abstract bool WaitForAnimatorToFulfill { get; }

        private bool hasMoverOrderFinished = true;
        private bool hasAnimationFinished = true;

        protected CreatureState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            hasMoverOrderFinished = !WaitForMoverToFulfill;
            hasAnimationFinished = !WaitForAnimatorToFulfill;
        }

        public override void Fulfil() {
            base.Fulfil();
            IsNextCinematicState = false;
        }

        public override void Interrupt() {
            base.Interrupt();
            AutomatedObject.Mover.InterruptCurrentOrder();
            AutomatedObject.Animator.InterruptCurrentOrder();
        }

        protected void OnMoverOrderFulfilled(bool wasInterrupted) {
            if (!WaitForMoverToFulfill) return;
            hasMoverOrderFinished = true;

            if (wasInterrupted) return;
            if (WaitForAnimatorToFulfill && !hasAnimationFinished) return;
            Fulfil();
        }

        protected void OnAnimationFinished(bool wasInterrupted) {
            if (!WaitForAnimatorToFulfill) return;
            hasAnimationFinished = true;

            if (wasInterrupted) return;
            if (WaitForMoverToFulfill && !hasMoverOrderFinished) return;
            Fulfil();
        }

        public override void Clear() {
            base.Clear();
            hasMoverOrderFinished = true;
            hasAnimationFinished = true;
        }
    }
}