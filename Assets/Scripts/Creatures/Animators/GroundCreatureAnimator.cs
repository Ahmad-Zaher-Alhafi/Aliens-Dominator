using UnityEngine;

namespace Creatures.Animators {
    public class GroundCreatureAnimator : CreatureAnimator {
        [SerializeField] protected AnimationClip walkAnimationClip;
        [SerializeField] protected AnimationClip runAnimationClip;
    }
}