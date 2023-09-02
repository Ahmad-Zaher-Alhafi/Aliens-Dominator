using System;
using UnityEngine;

namespace Creatures.Animators {
    public class CreatureMaganteeAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip rollAnimationClip;
        [SerializeField] private AnimationClip rollToIdleAnimationClip;
        [SerializeField] private AnimationClip spawnFromMouthAnimationClip;

        public override void PlaySpecialAbilityAnimation(Action<bool> informAnimationFinished) {
            base.PlaySpecialAbilityAnimation(informAnimationFinished);
            PlayAnimationClip(rollAnimationClip, false, informAnimationFinished);
        }

        public void PlayRollToIdleAnimation(Action<bool> informAnimationFinished) {
            PlayAnimationClip(rollToIdleAnimationClip, true, informAnimationFinished);
        }
    }
}