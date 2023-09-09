using System;
using Context;
using Creatures.Animators;
using FiniteStateMachine.CreatureStateMachine;

namespace Creatures {
    public class CreatureMagantee : GroundCreature {
        public override bool HasSpawningAnimation => IsStateActive<SpawningState>();

        protected override void Awake() {
            base.Awake();
            Ctx.Deps.EventsManager.PathPointReached += OnPathPointReached;
        }

        private void OnPathPointReached(Creature creature, PathPoint pathPoint) {
            if (creature != this) return;
            if (!IsStateActive<SpecialAbilityState>()) return;
            ((CreatureMaganteeAnimator) Animator).PlayRollToIdleAnimation(InformAnimationFinishedCallback);
        }

        public override void ExecuteSpecialAbility(Action<bool> informAnimationFinishedCallback) {
            base.ExecuteSpecialAbility(informAnimationFinishedCallback);
            // This special ability has multi animations, so do not call back once the first is finished as the call back will be played by the last animation
            Animator.PlaySpecialAbilityAnimation(null);
        }

        public override void OnDeath() {
            base.OnDeath();
            Ctx.Deps.EventsManager.PathPointReached -= OnPathPointReached;
        }
    }
}