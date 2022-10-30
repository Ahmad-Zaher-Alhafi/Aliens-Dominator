using UnityEngine;

namespace Creatures.Animators {
    public class CreatureUlifoAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip roarAnimationClip;
        [SerializeField] private AnimationClip feedAnimationClip;
    }
}