using UnityEngine;

namespace Creatures.Animators {
    public class CreatureGarooAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip lookAroundAnimationClip;
        [SerializeField] private AnimationClip feedAnimationClip;
        [SerializeField] private AnimationClip roarAnimationClip;

        private static readonly int idleIndexParameter = Animator.StringToHash("Idle Index");
        private int idleAnimationIndex;

        protected override void Update() {
            base.Update();
            if (Creature.CurrentState == Creature.CreatureState.Idle) {
                if (AnimationFinished) {
                    SetRandomIdleAnimation();
                }

                InterpolateFloatParameter(idleIndexParameter, idleAnimationIndex, ANIMATION_SWITCH_TIME);
            }
        }

        private void SetRandomIdleAnimation() {
            switch (Random.Range(1, 4)) {
                case 1: {
                    idleAnimationIndex = 1;
                    StartCoroutine(StopTheAnimationAfter(IdleAnimationClip.length));
                }
                    break;
                case 2: {
                    idleAnimationIndex = 2;
                    StartCoroutine(StopTheAnimationAfter(lookAroundAnimationClip.length));
                }
                    break;
                case 3: {
                    idleAnimationIndex = 3;
                    StartCoroutine(StopTheAnimationAfter(feedAnimationClip.length));
                }
                    break;
            }
        }
    }
}