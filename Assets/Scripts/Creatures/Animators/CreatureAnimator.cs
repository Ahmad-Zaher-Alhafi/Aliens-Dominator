using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Creatures.Animators {
    public abstract class CreatureAnimator : MonoBehaviour {
        [SerializeField] protected List<AnimationClip> PhysicalAttackAnimationClips;
        [FormerlySerializedAs("IdleAnimationClips")]
        [SerializeField] protected AnimationClip IdleAnimationClip;
        [SerializeField] protected AnimationClip takeDamageAnimationClip;
        [SerializeField] protected AnimationClip dieAnimationClip;


        protected Creature Creature;
        private Animator animator;
        private Action<bool> informAnimationFinishedCallback;
        private AnimationClip currentActiveAnimationClip;

        protected const float ANIMATION_SWITCH_TIME = 3;
        private static readonly int currentSpeedParameter = Animator.StringToHash("Current Speed");

        private void Awake() {
            Creature = GetComponent<Creature>();
            animator = GetComponent<Animator>();
        }

        public void Init() {
            animator.enabled = true;
        }

        protected virtual void Update() {
            animator.speed = Mathf.Clamp(Creature.CurrentSpeed / Creature.PatrolSpeed, 1, 1.5f);
            InterpolateFloatParameter(currentSpeedParameter, Creature.CurrentSpeed, ANIMATION_SWITCH_TIME);
        }

        public virtual void SetRandomIdleAnimation(Action<bool> informAnimationFinishedCallBack, float animationLength = 0) {
            informAnimationFinishedCallback = informAnimationFinishedCallBack;
            StartCoroutine(FulfilCurrentOrderOnceFinished(animationLength));
        }

        public void PlayAttackAnimation(Action<bool> informAnimationFinishedCallBack, Action informToAttack) {
            AnimationClip randomAttackAnimationClip = MathUtils.GetRandomObjectFromList(PhysicalAttackAnimationClips);
            PlayAnimationClip(randomAttackAnimationClip, informAnimationFinishedCallBack);
            StartCoroutine(InformToApplyDamageAfter(currentActiveAnimationClip.length, informToAttack));
        }

        public void PlayGettingHitAnimation(Action<bool> informAnimationFinishedCallBack) {
            PlayAnimationClip(takeDamageAnimationClip, informAnimationFinishedCallBack);
        }

        public virtual void PlaySpecialAbilityAnimation(Action<bool> informAnimationFinishedCallBack) { }

        /// <summary>
        /// To set a float parameter in a smooth way, useful in switching between idle, walking and running
        /// </summary>
        /// <param name="animationClipID"></param>
        /// <param name="newValue"></param>
        /// <param name="speed"></param>
        protected void InterpolateFloatParameter(int animationClipID, float newValue, float speed) {
            float oldValue = animator.GetFloat(animationClipID);
            float value = Mathf.Lerp(oldValue, newValue, speed * Time.deltaTime);
            animator.SetFloat(animationClipID, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="informAnimationFinishedCallBack">If true, the animator will send a call back to it's state that the animation has finished</param>
        protected void PlayAnimationClip(AnimationClip animationClip, Action<bool> informAnimationFinishedCallBack) {
            informAnimationFinishedCallback = informAnimationFinishedCallBack;
            ResetCurrentAnimationTrigger();
            currentActiveAnimationClip = animationClip;
            animator.SetTrigger(currentActiveAnimationClip.name);
            StartCoroutine(FulfilCurrentOrderOnceFinished(currentActiveAnimationClip.length));
        }

        private IEnumerator InformToApplyDamageAfter(float length, Action informToApplyDamage) {
            yield return new WaitForSeconds(length / 2);
            // Inform to attack only if the attack animation still playing as it could get interrupted by another animation during the wait time
            if (animator.GetCurrentAnimatorClipInfo(0).GetHashCode() != currentActiveAnimationClip.GetHashCode()) yield break;
            informToApplyDamage.Invoke();
            yield return new WaitForSeconds(length / 2);
        }

        private IEnumerator FulfilCurrentOrderOnceFinished(float length) {
            yield return new WaitForSeconds(length);
            ResetCurrentAnimationTrigger();
            informAnimationFinishedCallback?.Invoke(false);
        }

        private void ResetCurrentAnimationTrigger() {
            if (currentActiveAnimationClip == null) return;
            animator.ResetTrigger(currentActiveAnimationClip.name);
        }

        public void InterruptCurrentOrder() {
            informAnimationFinishedCallback?.Invoke(true);
        }

        public void OnDeath() {
            animator.enabled = false;
        }
    }
}