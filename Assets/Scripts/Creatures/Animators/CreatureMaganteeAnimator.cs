using System;
using UnityEngine;

namespace Creatures.Animators {
    public class CreatureMaganteeAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip rollAnimationClip;
        [SerializeField] private AnimationClip rollToIdleAnimationClip;
        [SerializeField] private AnimationClip spawnFromMouthAnimationClip;

        public override void PlaySpecialAbilityAnimation(Action<bool> informAnimationFinishedCallBack) {
            base.PlaySpecialAbilityAnimation(informAnimationFinishedCallBack);
            PlayAnimationClip(rollAnimationClip, informAnimationFinishedCallBack, true);
        }

        public void PlayRollToIdleAnimation(Action<bool> informAnimationFinishedCallBack) {
            PlayAnimationClip(rollToIdleAnimationClip, informAnimationFinishedCallBack);
        }
    }
}