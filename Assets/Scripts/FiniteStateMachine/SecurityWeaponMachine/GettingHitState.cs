using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GettingHitState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.GettingHit;
        public override bool CanBeActivated() => AutomatedObject.Health > 0 && gotHit;


        private bool gotHit;
        private int damageWeight;
        private IDamager damager;

        public GettingHitState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }


        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);

            int totalDamage = damager.Damage * damageWeight;
            AutomatedObject.OnDamageTaken(totalDamage);
            Debug.Log($"Creature {AutomatedObject} took damage = {totalDamage} and current health = {AutomatedObject.Health}");

            gotHit = false;

            Fulfil();
        }

        public void GotHit(IDamager damager, int damageWeight) {
            gotHit = true;
            this.damager = damager;
            this.damageWeight = damageWeight;
        }
    }
}