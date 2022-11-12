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
        
        /// <summary>
        /// To set a float parameter in a smooth way, useful in switching between idle, walking and walking
        /// </summary>
        /// <param name="animationClipID"></param>
        /// <param name="newValue"></param>
        /// <param name="speed"></param>
        protected void InterpolateFloatParameter(int animationClipID, float newValue, float speed) {
            float oldValue = Animator.GetFloat(animationClipID);
            float value = Mathf.Lerp(oldValue, newValue, speed * Time.deltaTime);
            Animator.SetFloat(animationClipID, value);
        }
    }
}