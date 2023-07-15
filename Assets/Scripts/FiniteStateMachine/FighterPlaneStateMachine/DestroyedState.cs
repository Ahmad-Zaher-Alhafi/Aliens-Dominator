using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class DestroyedState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Destroyed;
        public override bool CanBeActivated() => AutomatedObject.IsDestroyed;

        public DestroyedState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}