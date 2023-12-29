using Context;
using Creatures;
using ScriptableObjects;

namespace FiniteStateMachine.CreatureStateMachine {
    public class RunningAwayState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.RunningAway;
        public override bool CanBeActivated() => waveStarted && AutomatedObject.IsCinematic;
        public override float? Speed => AutomatedObject.RunSpeed;
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

        private void OnWaveStarted(Wave wave) {
            // Creatures will start running away once the wave is started
            waveStarted = true;
        }

        public override void Clear() {
            base.Clear();
            Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
            waveStarted = false;
        }
    }
}