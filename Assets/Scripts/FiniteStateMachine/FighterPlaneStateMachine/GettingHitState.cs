using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class GettingHitState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.GettingHit;
        public override bool CanBeActivated() => AutomatedObject.Health > 0 && gotHit;

        private bool gotHit;
        private int damageWeight;
        private IDamager damager;


        public GettingHitState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);

            int totalDamage = damager.Damage * damageWeight;
            AutomatedObject.OnDamageTaken(totalDamage);
            Debug.Log($"Creature {AutomatedObject} took damage = {totalDamage} and current health = {AutomatedObject.Health}");

            gotHit = false;
        }

        public void GotHit(IDamager damager, int damageWeight) {
            gotHit = true;
            this.damager = damager;
            this.damageWeight = damageWeight;
        }
    }
}