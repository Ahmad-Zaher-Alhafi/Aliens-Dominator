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
        [SerializeField] private float timeBetweenAttacks = 2;

        protected Creature Creature;
        protected Animator Animator;
        protected bool AnimationFinished = true;

        private bool IsBusy;
        protected const float ANIMATION_SWITCH_TIME = 3;

        private void Awake() {
            Creature = GetComponent<Creature>();
            Animator = GetComponent<Animator>();
        }

        public void Init() {
            Animator.enabled = true;
        }

        protected virtual void Update() {
            Animator.speed = Mathf.Clamp(Creature.Mover.CurrentSpeed / Creature.Mover.PatrolSpeed, 1, 1.5f);

            if (Creature.CurrentState == Creature.CreatureState.Attacking) {
                if (AnimationFinished) {
                    SetRandomAttackAnimationParameter();
                }
            }
        }

        private void SetRandomAttackAnimationParameter() {
            AnimationClip randomAttackClip = MathUtils.GetRandomObjectFromList(PhysicalAttackAnimationClips);
            Animator.SetTrigger(randomAttackClip.name);
            StartCoroutine(StopTheAnimationAfter(randomAttackClip.length + timeBetweenAttacks));
            StartCoroutine(InformToAttackAfter(randomAttackClip.length));
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

        private IEnumerator InformToAttackAfter(float length) {
            yield return new WaitForSeconds(length / 2);
            Creature.ApplyDamage();
            yield return new WaitForSeconds(length / 2);
        }

        protected IEnumerator StopTheAnimationAfter(float length) {
            AnimationFinished = false;
            yield return new WaitForSeconds(length);
            AnimationFinished = true;
        }

        public void OnDeath() {
            Animator.enabled = false;
        }
    }
}