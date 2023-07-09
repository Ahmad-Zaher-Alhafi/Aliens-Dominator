using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class ShootingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Shooting;
        public override bool CanBeActivated() => false;

        public ShootingState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}