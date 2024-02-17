using System;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Creatures.Animators {
    public class CreatureGarooAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip lookAroundAnimationClip;
        [SerializeField] private AnimationClip feedAnimationClip;
        [SerializeField] private AnimationClip roarAnimationClip;

        private static readonly int idleIndexParameter = Animator.StringToHash("Idle Index");
        private int idleAnimationIndex;

        protected override void Update() {
            if (!IsServer) return;
            base.Update();
            if (Creature.IsStateActive<IdleState>()) {
                InterpolateFloatParameter(idleIndexParameter, idleAnimationIndex, ANIMATION_SWITCH_TIME);
            }
        }

        /// <summary>
        /// Garoo creature inherits this because it has multi idle animations
        /// </summary>
        public override void SetRandomIdleAnimation(Action<bool> informAnimationFinishedCallBack, float animationLength = 0) {
            switch (Random.Range(1, 4)) {
                case 1: {
                    idleAnimationIndex = 1;
                    animationLength = IdleAnimationClip.length;
                }
                    break;
                case 2: {
                    idleAnimationIndex = 2;
                    animationLength = lookAroundAnimationClip.length;
                }
                    break;
                case 3: {
                    idleAnimationIndex = 3;
                    animationLength = feedAnimationClip.length;
                }
                    break;
            }
            base.SetRandomIdleAnimation(informAnimationFinishedCallBack, animationLength);
        }
    }
}