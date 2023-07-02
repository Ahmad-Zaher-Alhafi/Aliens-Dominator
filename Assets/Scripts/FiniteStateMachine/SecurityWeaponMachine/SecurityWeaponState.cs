using FiniteStateMachine.CreatureStateMachine;
using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public enum SecurityWeaponStateType {
        Shutdown,
        Guarding,
        Aiming,
        Shooting,
        Destroyed,
    }

    public abstract class SecurityWeaponState : State<SecurityWeapon, SecurityWeaponStateType> {
        protected SecurityWeaponState(SecurityWeapon automatedObject) : base(automatedObject) { }
    }
}