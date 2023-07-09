using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class LandingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Landing;
        public override bool CanBeActivated() => false;

        public LandingState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}