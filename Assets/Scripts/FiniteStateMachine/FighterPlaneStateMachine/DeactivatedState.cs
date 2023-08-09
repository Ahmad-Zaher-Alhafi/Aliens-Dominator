using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class DeactivatedState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Deactivated;
        public override bool CanBeActivated() => AutomatedObject.HasLanded && AutomatedObject.CurrentStateType is not FighterPlaneStateType.Deactivated;

        public DeactivatedState(FighterPlane fighterPlane) : base(fighterPlane) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            Fulfil();
        }
    }
}