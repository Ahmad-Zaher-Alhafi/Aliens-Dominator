using Context;
using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class DestroyedState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Destroyed;
        public override bool CanBeActivated() => AutomatedObject.Health <= 0;

        public DestroyedState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            Ctx.Deps.ConstructionController.BulldozeWeapon(AutomatedObject);
        }
    }
}