using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class DestroyedState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Destroyed;
        public override bool CanBeActivated() => false;

        public DestroyedState(SecurityWeapon<TEnemyType> securityWeapon) : base(securityWeapon) { }
    }
}