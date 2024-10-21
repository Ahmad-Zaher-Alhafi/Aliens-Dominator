using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class GettingHitState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.GettingHit;
        public override bool CanBeActivated() => !AutomatedObject.IsDestroyed && gotHit;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;


        private bool gotHit;
        private int damage;
        private BodyPart.CreatureBodyPart damagedBodyPart;

        public GettingHitState(Creature creature, bool checkWhenAutomatingDisabled) : base(creature, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);

            AutomatedObject.OnDamageTaken(damage, damagedBodyPart, OnAnimationFinished);
            Debug.Log($"Creature {AutomatedObject} took damage = {damage} and current health = {AutomatedObject.Health}");

            gotHit = false;

            Ctx.Deps.EventsManager.TriggerEnemyGotHit(AutomatedObject);
        }

        public void GotHit(int damage, BodyPart.CreatureBodyPart damagedBodyPart) {
            gotHit = true;
            this.damage = damage;
            this.damagedBodyPart = damagedBodyPart;
        }
    }
}