using Context;
using Creatures;

namespace FiniteStateMachine.States {
    public class RunningAwayState : State {
        public override StateType Type => StateType.RunningAway;
        public override bool CanBeActivated() => waveStarted && Creature.IsCinematic;
        public override bool IsCinematic => true;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;
        

        private bool waveStarted;

        public RunningAwayState(bool isFinal, Creature creature) : base(isFinal, creature) {
            Ctx.Deps.EventsManager.WaveStarted += RunAway;
        }

        public override void Activate() {
            base.Activate();
            Creature.Mover.RunAway(OnMoverOrderFulfilled);
        }

        public override void Fulfil() {
            base.Fulfil();
            Creature.HasToDisappear = true;
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