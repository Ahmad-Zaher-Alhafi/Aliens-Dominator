using UnityEngine;

namespace Creatures.Animators {
    public class CreatureSerpentAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip roarAnimationClip;
        [SerializeField] private AnimationClip defendAnimationClip;
        [SerializeField] private AnimationClip castSpellAnimationClip;
    }
}