using UnityEngine;

namespace Creatures.Animators {
    public class CreatureSicklusAnimator : FlyCreatureAnimator {
        [SerializeField] private AnimationClip dodgeAnimationClip;
        [SerializeField] private AnimationClip castSpellAnimationClip;
    }
}