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

    public abstract class SecurityWeaponState : State<SecurityWeapon> {
        public abstract SecurityWeaponStateType Type { get; }
        protected SecurityWeaponState(SecurityWeapon stateObject) : base(stateObject) { }
    }
}