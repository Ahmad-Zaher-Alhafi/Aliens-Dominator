using System;
using UnityEngine;

namespace Creatures.Animators {
    public class FlyCreatureAnimator : CreatureAnimator {
        [SerializeField] protected AnimationClip flyAnimationClip;
        private static readonly int flyForwardParameter = Animator.StringToHash("Fly Forward");

        protected override void Update() {
            base.Update();
            switch (Creature.CurrentState) {
                case Creature.CreatureState.Idle:
                    Animator.SetBool(flyForwardParameter, false);
                    break;
                case Creature.CreatureState.Patrolling:
                    Animator.SetBool(flyForwardParameter, true);
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
                case Creature.CreatureState.None:
                    break;
                case Creature.CreatureState.RunningAway:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}