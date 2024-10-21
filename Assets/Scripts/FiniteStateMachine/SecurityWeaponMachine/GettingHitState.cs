using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GettingHitState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.GettingHit;
        public override bool CanBeActivated() => AutomatedObject.Health > 0 && gotHit;

        private bool gotHit;
        private int damage;

        public GettingHitState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }


        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);

            AutomatedObject.OnDamageTaken(damage);
            Debug.Log($"Creature {AutomatedObject} took damage = {damage} and current health = {AutomatedObject.Health}");

            gotHit = false;

            Fulfil();
        }

        public void GotHit(int damage) {
            gotHit = true;
            this.damage = damage;
        }
    }
}