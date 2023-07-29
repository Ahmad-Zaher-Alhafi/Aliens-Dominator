using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class DeadState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Dead;
        public override bool CanBeActivated() => AutomatedObject.Health <= 0 || AutomatedObject.HasToDisappear;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => false;

        public DeadState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            if (AutomatedObject.Health <= 0) {
                Die();
            } else if (AutomatedObject.HasToDisappear) {
                AutomatedObject.Disappear();
            }
        }

        private void Die() {
            Ctx.Deps.SupplyBalloonController.SpawnBalloon(AutomatedObject.transform.position, AutomatedObject.ChanceOfDroppingBalloon);

            // Force to push the creature away once get killed (More realistic)
            if (AutomatedObject.ObjectDamagedWith.HasPushingForce) {
                AutomatedObject.Rig.AddForce(AutomatedObject.ObjectDamagedWith.Transform.forward * AutomatedObject.PushForceWhenDead);
            }

            AutomatedObject.PlayDeathSound();
            AutomatedObject.OnDeath();
            Debug.Log($"Creature {AutomatedObject} is dead");
        }
    }
}