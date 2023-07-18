using Context;
using Creatures;

namespace FiniteStateMachine.CreatureStateMachine {
    public class RunningAwayState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.RunningAway;
        public override bool CanBeActivated() => waveStarted && AutomatedObject.IsCinematic;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;
        

        private bool waveStarted;

        public RunningAwayState(Creature creature) : base(creature) {
            Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
        }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Mover.RunAway(OnMoverOrderFulfilled);
        }

        public override void Fulfil() {
            base.Fulfil();
            AutomatedObject.HasToDisappear = true;
        }

        private void OnWaveStarted(int waveIndex) {
            // Creatures will start running away once the wave is started
            waveStarted = true;
        }

        protected override void Clear() {
            base.Clear();
            Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
            waveStarted = false;
        }
    }
}