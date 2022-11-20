using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Creatures.Animators {
    public abstract class CreatureAnimator : MonoBehaviour {
        [SerializeField] protected List<AnimationClip> PhysicalAttackAnimationClips;
        [FormerlySerializedAs("IdleAnimationClips")]
        [SerializeField] protected AnimationClip IdleAnimationClip;
        [SerializeField] protected AnimationClip takeDamageAnimationClip;
        [SerializeField] protected AnimationClip dieAnimationClip;

        protected Creature Creature;
        protected Animator Animator;
        protected float InitialSpeed;

        private bool IsBusy;
        protected const float ANIMATION_SWITCH_TIME = 3;

        private void Awake() {
            Creature = GetComponent<Creature>();
            Animator = GetComponent<Animator>();
            InitialSpeed = Animator.speed;
        }

        public void Init() {
            Animator.enabled = true;
        }

        protected virtual void Update() {
            Animator.speed = Mathf.Clamp(Creature.mover.CurrentSpeed / Creature.mover.PatrolSpeed, 1, 1.5f);
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

        public void OnDeath() {
            Animator.enabled = false;
        }
    }
}