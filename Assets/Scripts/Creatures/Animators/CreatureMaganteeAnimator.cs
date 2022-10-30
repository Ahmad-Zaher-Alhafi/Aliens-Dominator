using UnityEngine;

namespace Creatures.Animators {
    public class CreatureMaganteeAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip rollAnimationClip;
        [SerializeField] private AnimationClip rollToIdleAnimationClip;
        [SerializeField] private AnimationClip spawnFromMouthAnimationClip;
    }
}