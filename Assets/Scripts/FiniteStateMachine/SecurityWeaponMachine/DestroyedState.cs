using FiniteStateMachine.CreatureStateMachine;
using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class DestroyedState : SecurityWeaponState {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Destroyed;
        public override bool CanBeActivated() => false;

        public DestroyedState(SecurityWeapon securityWeapon) : base(securityWeapon) { }
    }
}