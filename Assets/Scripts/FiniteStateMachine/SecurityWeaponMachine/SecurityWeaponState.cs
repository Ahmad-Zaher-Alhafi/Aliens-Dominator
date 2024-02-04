using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public enum SecurityWeaponStateType {
        Shutdown,
        Guarding,
        Aiming,
        Shooting,
        Destroyed,
    }

    public abstract class SecurityWeaponState<TEnemyType> : State<SecurityWeapon<TEnemyType>, SecurityWeaponStateType> where TEnemyType : IAutomatable {
        protected SecurityWeaponState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }
    }
}