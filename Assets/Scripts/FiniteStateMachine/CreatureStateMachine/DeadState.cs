using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class DeadState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Dead;
        public override bool CanBeActivated() => StateObject.Health <= 0 || StateObject.HasToDisappear;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => false;

        public DeadState(Creature creature) : base(creature) { }

        public override void Activate() {
            base.Activate();
            if (StateObject.Health <= 0) {
                Die();
            } else if (StateObject.HasToDisappear) {
                StateObject.Disappear();
            }
        }

        private void Die() {
            StateObject.IsDead = true;

            Ctx.Deps.SupplyBalloonController.SpawnBalloon(StateObject.transform.position, StateObject.ChanceOfDroppingBalloon);

            // Force to push the creature away once get killed (More realistic)
            if (StateObject.ObjectDamagedWith.HasPushingForce) {
                StateObject.Rig.AddForce(StateObject.ObjectDamagedWith.Transform.forward * StateObject.PushForceWhenDead);
            }
           
            StateObject.PlayDeathSound();
            StateObject.Health = StateObject.InitialHealth;
            StateObject.OnDeath();
            Debug.Log($"Creature {StateObject} is dead");
        }
    }
}