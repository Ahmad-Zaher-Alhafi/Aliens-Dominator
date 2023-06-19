using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class GettingHitState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.GettingHit;
        public override bool CanBeActivated() => StateObject.Health > 0 && gotHit;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;
        
        
        private bool gotHit;
        private int damageWeight;
        private IDamager damager;

        public GettingHitState(Creature objectToState) : base(objectToState) { }

        public override void Activate() {
            base.Activate();

            int totalDamage = damager.Damage * damageWeight;
            StateObject.Health -= totalDamage;
            Debug.Log($"Creature {StateObject} took damage = {totalDamage} and current health = {StateObject.Health}");

            StateObject.Animator.PlayGettingHitAnimation(OnAnimationFinished);

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