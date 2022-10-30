using UnityEngine;

namespace Creatures.Animators {
    public class CreatureMagantisAnimator : GroundCreatureAnimator {
        [SerializeField] private AnimationClip roarAnimationClip;
        [SerializeField] private AnimationClip spawnBugFromMouthAnimationClip;
    }
}