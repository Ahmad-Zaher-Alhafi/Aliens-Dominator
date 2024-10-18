using Context;
using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class DestroyedState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Destroyed;
        public override bool CanBeActivated() => AutomatedObject.Health <= 0;

        public DestroyedState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            Ctx.Deps.ConstructionController.BulldozeWeapon(AutomatedObject);
        }
    }
}