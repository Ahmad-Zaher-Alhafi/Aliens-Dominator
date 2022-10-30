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
        
        private static readonly int currentSpeedParameter = Animator.StringToHash("Current Speed");
        private bool IsBusy;
        protected const float ANIMATION_SWITCH_TIME = 3;
        
        private void Awake() {
            Creature = GetComponent<Creature>();
            Animator = GetComponent<Animator>();
            InitialSpeed = Animator.speed;
        }

        protected virtual void Update() {
            InterpolateFloatParameter(currentSpeedParameter, Creature.CreatureMover.CurrentSpeed, ANIMATION_SWITCH_TIME);

            switch (Creature.CurrentState) {
                case Creature.CreatureState.Idle:
                    break;
                case Creature.CreatureState.Patrolling:
                    break;
                case Creature.CreatureState.FollowingPath:
                    break;
                case Creature.CreatureState.GettingHit:
                    break;
                case Creature.CreatureState.Attacking:
                    break;
                case Creature.CreatureState.Chasing:
                    break;
                case Creature.CreatureState.Dead:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        protected void InterpolateFloatParameter(int animationClipID, float newValue, float speed) {
            float oldValue = Animator.GetFloat(animationClipID);
            float value = Mathf.Lerp(oldValue, newValue, speed * Time.deltaTime);
            Animator.SetFloat(animationClipID, value);
        }
    }
}