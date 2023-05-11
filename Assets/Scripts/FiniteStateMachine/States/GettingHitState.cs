using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.States {
    public class GettingHitState : State {
        public override StateType Type => StateType.GettingHit;
        public override bool CanBeActivated() => Creature.Health > 0 && gotHit;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;
        
        
        private bool gotHit;
        private int damageWeight;
        private IDamager damager;

        public GettingHitState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();

            int totalDamage = damager.Damage * damageWeight;
            Creature.Health -= totalDamage;
            Debug.Log($"Creature {Creature} took damage = {totalDamage} and current health = {Creature.Health}");

            Creature.Animator.PlayGettingHitAnimation(OnAnimationFinished);

            Ctx.Deps.CreatureSpawnController.OnCreatureHit();
            
            gotHit = false;
        }

        public void GotHit(IDamager damager, int damageWeight) {
            gotHit = true;
            this.damager = damager;
            this.damageWeight = damageWeight;
        }
    }
}