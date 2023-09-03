using System;
using UnityEngine;

namespace Creatures.Animators {
    public class CreatureMagantisAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip roarAnimationClip;
        [SerializeField] private AnimationClip spawnBugFromMouthAnimationClip;

        public override void PlaySpecialAbilityAnimation(Action<bool> informAnimationFinishedCallBack) {
            base.PlaySpecialAbilityAnimation(informAnimationFinishedCallBack);
            PlayAnimationClip(spawnBugFromMouthAnimationClip, informAnimationFinishedCallBack);
        }
    }
}