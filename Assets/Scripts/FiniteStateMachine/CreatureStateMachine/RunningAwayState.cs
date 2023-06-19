using Context;
using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class RunningAwayState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.RunningAway;
        public override bool CanBeActivated() => waveStarted && StateObject.IsCinematic;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;
        

        private bool waveStarted;

        public RunningAwayState(Creature creature) : base(creature) {
            Ctx.Deps.EventsManager.WaveStarted += RunAway;
        }

        public override void Activate() {
            base.Activate();
            StateObject.Mover.RunAway(OnMoverOrderFulfilled);
        }

        public override void Fulfil() {
            base.Fulfil();
            StateObject.HasToDisappear = true;
        }

        private void RunAway() {
            waveStarted = true;
        }

        protected override void Clear() {
            base.Clear();
            Ctx.Deps.EventsManager.WaveStarted -= RunAway;
            waveStarted = false;
        }
    }
}