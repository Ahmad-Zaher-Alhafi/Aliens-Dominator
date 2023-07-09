using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class AimingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Aiming;
        public override bool CanBeActivated() => false;

        public AimingState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}