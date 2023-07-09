using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class DeactivatedState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Deactivated;
        public override bool CanBeActivated() => false;

        public DeactivatedState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}