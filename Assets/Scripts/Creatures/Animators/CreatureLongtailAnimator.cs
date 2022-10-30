using UnityEngine;

namespace Creatures.Animators {
    public class CreatureLongtailAnimator : FlyCreatureAnimator {
        [SerializeField] private AnimationClip dodgeAnimationClip;
        [SerializeField] private AnimationClip castSpellAnimationClip;
    }
}