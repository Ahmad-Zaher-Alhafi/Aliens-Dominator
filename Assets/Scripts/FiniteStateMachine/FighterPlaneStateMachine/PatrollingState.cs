using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class PatrollingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Patrolling;
        public override bool CanBeActivated() => false;

        public PatrollingState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}