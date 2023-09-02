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
            base.Update();
            if (Creature.IsStateActive<IdleState>()) {
                InterpolateFloatParameter(idleIndexParameter, idleAnimationIndex, ANIMATION_SWITCH_TIME);
            }
        }

        public override void SetRandomIdleAnimation(Action<bool> informAnimationFinished) {
            base.SetRandomIdleAnimation(informAnimationFinished);
            switch (Random.Range(1, 4)) {
                case 1: {
                    idleAnimationIndex = 1;
                    StartCoroutine(InformAnimationFinishedAfter(IdleAnimationClip.length));
                }
                    break;
                case 2: {
                    idleAnimationIndex = 2;
                    StartCoroutine(InformAnimationFinishedAfter(lookAroundAnimationClip.length));
                }
                    break;
                case 3: {
                    idleAnimationIndex = 3;
                    StartCoroutine(InformAnimationFinishedAfter(feedAnimationClip.length));
                }
                    break;
            }
        }
    }
}