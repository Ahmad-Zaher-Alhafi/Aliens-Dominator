using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class ShutdownState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Shutdown;
        public override bool CanBeActivated() => true;

        public ShutdownState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }
    }
}