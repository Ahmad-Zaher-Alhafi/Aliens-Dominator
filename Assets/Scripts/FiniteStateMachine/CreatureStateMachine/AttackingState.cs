﻿using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class AttackingState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Attacking;
        public override bool CanBeActivated() => AutomatedObject.TargetReached && Time.time > lastTimeActivated + 2;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;
        private float lastTimeActivated;

        public AttackingState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Animator.PlayAttackAnimation(OnAnimationFinished, ApplyDamage);
            lastTimeActivated = Time.time;
        }

        private void ApplyDamage() {
            Debug.Log($"Creature {AutomatedObject} applies damage!");
        }
    }
}