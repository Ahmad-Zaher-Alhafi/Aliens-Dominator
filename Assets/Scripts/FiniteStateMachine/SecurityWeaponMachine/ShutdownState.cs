using FiniteStateMachine.CreatureStateMachine;
using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class ShutdownState : SecurityWeaponState {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Shutdown;
        public override bool CanBeActivated() => true;

        public ShutdownState(SecurityWeapon securityWeapon) : base(securityWeapon) { }
    }
}