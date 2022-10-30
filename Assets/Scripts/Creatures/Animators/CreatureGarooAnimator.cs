using System.Collections;
using UnityEngine;

namespace Creatures.Animators {
    public class CreatureGarooAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip lookAroundAnimationClip;
        [SerializeField] private AnimationClip feedAnimationClip;
        [SerializeField] private AnimationClip roarAnimationClip;

        private static readonly int idleIndexParameter = Animator.StringToHash("Idle Index");
        private int idleAnimationIndex;
        private bool idleAnimationFinished = true;

        protected override void Update() {
            base.Update();
            if (Creature.CurrentState == Creature.CreatureState.Idle) {
                if (idleAnimationFinished) {
                    SetRandomIdleAnimation();
                }

                InterpolateFloatParameter(idleIndexParameter, idleAnimationIndex, ANIMATION_SWITCH_TIME);
            }
        }

        private void SetRandomIdleAnimation() {
            switch (Random.Range(1, 4)) {
                case 1: {
                    idleAnimationIndex = 1;
                    StartCoroutine(WaitForAnimationLength(IdleAnimationClip.length));
                }
                    break;
                case 2: {
                    idleAnimationIndex = 2;
                    StartCoroutine(WaitForAnimationLength(lookAroundAnimationClip.length));
                }
                    break;
                case 3: {
                    idleAnimationIndex = 3;
                    StartCoroutine(WaitForAnimationLength(feedAnimationClip.length));
                }
                    break;
            }


        }

        private IEnumerator WaitForAnimationLength(float length) {
            idleAnimationFinished = false;
            yield return new WaitForSeconds(length);
            idleAnimationFinished = true;
        }
    }
}