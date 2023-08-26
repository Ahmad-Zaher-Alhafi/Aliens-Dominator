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
            animator.speed = Mathf.Clamp(Creature.Mover.CurrentSpeed / Creature.Mover.PatrolSpeed, 1, 1.5f);
            InterpolateFloatParameter(currentSpeedParameter, Creature.Mover.CurrentSpeed, ANIMATION_SWITCH_TIME);
        }

        public virtual void PlayIdleAnimation(Action<bool> informAnimationFinished) {
            informAnimationFinishedCallback = informAnimationFinished;
        }

        public void PlayAttackAnimation(Action<bool> informAnimationFinished, Action informToAttack) {
            informAnimationFinishedCallback = informAnimationFinished;
            AnimationClip randomAttackAnimationClip = MathUtils.GetRandomObjectFromList(PhysicalAttackAnimationClips);
            PlayAnimationClip(randomAttackAnimationClip);
            StartCoroutine(InformToApplyDamageAfter(currentActiveAnimationClip.length, informToAttack));
        }

        public void PlayGettingHitAnimation(Action<bool> informAnimationFinished) {
            informAnimationFinishedCallback = informAnimationFinished;
            ResetCurrentAnimationTrigger();
            PlayAnimationClip(takeDamageAnimationClip);
        }

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

        private void PlayAnimationClip(AnimationClip animationClip) {
            currentActiveAnimationClip = animationClip;
            animator.SetTrigger(currentActiveAnimationClip.name);
            currentActiveAnimationClip = takeDamageAnimationClip;
            StartCoroutine(InformAnimationFinishedAfter(currentActiveAnimationClip.length));
        }

        private IEnumerator InformToApplyDamageAfter(float length, Action informToApplyDamage) {
            yield return new WaitForSeconds(length / 2);
            // Inform to attack only if the attack animation still playing as it could get interrupted by another animation during the wait time
            if (animator.GetCurrentAnimatorClipInfo(0).GetHashCode() != currentActiveAnimationClip.GetHashCode()) yield break;
            informToApplyDamage.Invoke();
            yield return new WaitForSeconds(length / 2);
        }

        protected IEnumerator InformAnimationFinishedAfter(float length) {
            yield return new WaitForSeconds(length);
            FulfillCurrentOrder();
        }

        private void ResetCurrentAnimationTrigger() {
            if (currentActiveAnimationClip == null) return;
            animator.ResetTrigger(currentActiveAnimationClip.name);
        }

        private void FulfillCurrentOrder() {
            informAnimationFinishedCallback?.Invoke(false);
        }

        public void InterruptCurrentOrder() {
            informAnimationFinishedCallback?.Invoke(true);
        }

        public void OnDeath() {
            animator.enabled = false;
        }
    }
}