using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class GettingHitState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.GettingHit;
        public override bool CanBeActivated() => AutomatedObject.Health > 0 && gotHit;

        private bool gotHit;
        private int damage;

        public GettingHitState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

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