using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class AttackingState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Attacking;
        public override bool CanBeActivated() => StateObject.TargetReached && Time.time > lastTimeActivated + 2;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;
        private float lastTimeActivated;

        public AttackingState(Creature creature) : base(creature) { }

        public override void Activate() {
            base.Activate();
            StateObject.Animator.PlayAttackAnimation(OnAnimationFinished, ApplyDamage);
            lastTimeActivated = Time.time;
        }

        private void ApplyDamage() {
            Debug.Log($"Creature {StateObject} applies damage!");
        }
    }
}