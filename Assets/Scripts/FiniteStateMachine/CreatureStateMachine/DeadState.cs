﻿using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class DeadState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Dead;
        public override bool CanBeActivated() => AutomatedObject.Health <= 0 || AutomatedObject.HasToDisappear;
        public override float? Speed => 0;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => false;

        public DeadState(Creature creature, bool checkWhenAutomatingDisabled) : base(creature, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            if (AutomatedObject.Health <= 0) {
                Die();
            } else if (AutomatedObject.HasToDisappear) {
                AutomatedObject.Disappear();
            }
        }

        private void Die() {
            AutomatedObject.OnDeath();
            Debug.Log($"Creature {AutomatedObject} is dead");
        }
    }
}