using UnityEngine;

namespace Creatures.Animators {
    public class GroundCreatureAnimator : CreatureAnimator {
        [SerializeField] protected AnimationClip walkAnimationClip;
        [SerializeField] protected AnimationClip runAnimationClip;
        
        private static readonly int currentSpeedParameter = Animator.StringToHash("Current Speed");
        
        protected override void Update() {
            base.Update();
            InterpolateFloatParameter(currentSpeedParameter, Creature.mover.CurrentSpeed, ANIMATION_SWITCH_TIME);
        }
    }
}