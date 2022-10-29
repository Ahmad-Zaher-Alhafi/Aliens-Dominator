using System;
using UnityEngine;

namespace Creatures {
    public abstract class CreatureMover : MonoBehaviour {
        public bool IsBusy { get; protected set; }

        [SerializeField] private float speed = 3;
        [SerializeField] private float rotatingSpeed = 1;
        protected float Speed { get => speed; set => speed = value; }
        protected float RotatingSpeed { get => rotatingSpeed; set => rotatingSpeed = value; }
        protected virtual void StayIdle() { }
        protected abstract void FollowPath();
        protected abstract void Patrol();

        protected Creature Creature;
        protected bool HasMovingOrder;
        
        protected virtual void Awake() {
            Creature = GetComponent<Creature>();
        }

        protected virtual void Update() {
            if (IsBusy) return;
            
            switch (Creature.CurrentState) {
                case Creature.CreatureState.Idle:
                    StayIdle();
                    break;
                case Creature.CreatureState.Patrolling:
                    Patrol();
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
        }
    }
}