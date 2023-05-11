using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.States {
    public class DeadState : State {
        public override StateType Type => StateType.Dead;
        public override bool CanBeActivated() => Creature.Health <= 0 || Creature.HasToDisappear;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => false;

        public DeadState(bool isFinal, Creature creature) : base(isFinal, creature) { }

        public override void Activate() {
            base.Activate();
            if (Creature.Health <= 0) {
                Die();
            } else if (Creature.HasToDisappear) {
                Creature.Disappear();
            }
        }

        private void Die() {
            Creature.IsDead = true;

            Ctx.Deps.SupplyBalloonController.SpawnBalloon(Creature.transform.position, Creature.ChanceOfDroppingBalloon);

            // Force to push the creature away once get killed (More realistic)
            Creature.Rig.AddForce(Creature.ObjectDamagedWith.Transform.forward * Creature.PushForceWhenDead);

            Creature.PlayDeathSound();
            Creature.Rig.useGravity = true;
            Creature.Rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Creature.Health = Creature.InitialHealth;
            Creature.OnDeath();
            Debug.Log($"Creature {Creature} is dead");
        }
    }
}