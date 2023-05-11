using Creatures;
using UnityEngine;

namespace FiniteStateMachine.States {
    public class AttackingState : State {
        public override StateType Type => StateType.Attacking;
        public override bool CanBeActivated() => Creature.TargetReached && Time.time > lastTimeActivated + 2;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;
        private float lastTimeActivated;

        public AttackingState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();
            Creature.Animator.PlayAttackAnimation(OnAnimationFinished, ApplyDamage);
            lastTimeActivated = Time.time;
        }

        private void ApplyDamage() {
            Debug.Log($"Creature {Creature} applies damage!");
        }
    }
}