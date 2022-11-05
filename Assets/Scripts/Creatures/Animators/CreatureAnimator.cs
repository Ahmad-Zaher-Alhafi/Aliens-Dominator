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
            if (Creature.CurrentState == Creature.CreatureState.Dead) {
                Animator.enabled = false;
                return;
            }

            switch (Creature.IsSlowedDown) {
                case true when Animator.speed.Equals(InitialSpeed):
                    Animator.speed /= 2;
                    break;
                case false when !Animator.speed.Equals(InitialSpeed):
                    Animator.speed = InitialSpeed;
                    break;
            }

            /*if (Creature.PreviousState == Creature.CreatureState.GettingHit) {
                Animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.TakeDamage));
            } else if (Creature.PreviousState == Creature.CreatureState.Dead) {
                Animator.enabled = !enabled;
            }*/

        }
    }
}