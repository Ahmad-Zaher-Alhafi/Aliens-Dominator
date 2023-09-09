using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class SpawningState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Spawning;
        public override bool CanBeActivated() => AutomatedObject.IsStateActive<NoneState>();
        public override float? Speed => 0;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => AutomatedObject.HasSpawningAnimation;

        public SpawningState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            if (AutomatedObject.HasSpawningAnimation) {
                AutomatedObject.Animator.PlaySpawnAnimation(OnAnimationFinished);
            } else {
                Fulfil();
            }
        }
    }
}