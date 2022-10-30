using UnityEngine;

namespace Creatures.Animators {
    public class CreatureTelekinisAnimator : FlyCreatureAnimator {
        [SerializeField] private AnimationClip meditateAnimationClip;
        [SerializeField] private AnimationClip mindControlAnimationClip;
        [SerializeField] private AnimationClip flyAwayAnimationClip;
        [SerializeField] private AnimationClip defendForceFieldAnimationClip;
        [SerializeField] private AnimationClip spawnFromSkyAnimationClip;
        [SerializeField] private AnimationClip castSpellAnimationClip;
    }
}