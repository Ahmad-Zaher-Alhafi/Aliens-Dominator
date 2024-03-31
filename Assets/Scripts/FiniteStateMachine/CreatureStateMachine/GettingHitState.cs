using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class GettingHitState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.GettingHit;
        public override bool CanBeActivated() => AutomatedObject.Health > 0 && gotHit;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;


        private bool gotHit;
        private int damageWeight;
        private IDamager damager;
        private BodyPart.CreatureBodyPart damagedBodyPart;

        public GettingHitState(Creature creature, bool checkWhenAutomatingDisabled) : base(creature, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);

            int totalDamage = damager.Damage * damageWeight;
            AutomatedObject.Health -= totalDamage;
            AutomatedObject.OnDamageTaken(totalDamage, damagedBodyPart);
            Debug.Log($"Creature {AutomatedObject} took damage = {totalDamage} and current health = {AutomatedObject.Health}");

            AutomatedObject.Animator.PlayGettingHitAnimation(OnAnimationFinished);

            gotHit = false;

            Ctx.Deps.EventsManager.TriggerEnemyGotHit(AutomatedObject);
        }

        public void GotHit(IDamager damager, int damageWeight, BodyPart.CreatureBodyPart damagedBodyPart) {
            gotHit = true;
            this.damager = damager;
            this.damageWeight = damageWeight;
            this.damagedBodyPart = damagedBodyPart;
        }
    }
}