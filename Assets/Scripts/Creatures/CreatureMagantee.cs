using System;
using Context;
using Creatures.Animators;
using FiniteStateMachine.CreatureStateMachine;

namespace Creatures {
    public class CreatureMagantee : GroundCreature {
        private Action<bool> informAnimationFinishedCallback;

        protected override void Awake() {
            base.Awake();
            Ctx.Deps.EventsManager.PathPointReached += OnPathPointReached;
        }

        private void OnPathPointReached(Creature creature, PathPoint pathPoint) {
            if (creature != this) return;
            if (!IsStateActive<SpecialAbilityState>()) return;
            ((CreatureMaganteeAnimator) Animator).PlayRollToIdleAnimation(informAnimationFinishedCallback);
        }

        public override void ExecuteSpecialAbility(Action<bool> informAnimationFinishedCallback) {
            base.ExecuteSpecialAbility(informAnimationFinishedCallback);
            this.informAnimationFinishedCallback = informAnimationFinishedCallback;
            Animator.PlaySpecialAbilityAnimation(informAnimationFinishedCallback);
        }

        public override void OnDeath() {
            base.OnDeath();
            Ctx.Deps.EventsManager.PathPointReached -= OnPathPointReached;
        }
    }
}