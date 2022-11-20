using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creatures {
    public abstract class CreatureMover : MonoBehaviour {
        public bool IsBusy { get; protected set; }
        public float CurrentSpeed { get; private set; }

        [SerializeField] private float secondsToStayIdle = 5;
        [SerializeField] private float patrolSpeed = 3;
        public float PatrolSpeed => patrolSpeed;
        [SerializeField] private float runSpeed = 6;
        [SerializeField] private float rotatingSpeed = 1;
        
        protected float RotatingSpeed { get => rotatingSpeed; set => rotatingSpeed = value; }
        protected Creature Creature;
        protected bool HasMovingOrder;

        protected virtual void Awake() {
            Creature = GetComponent<Creature>();
        }

        public virtual void Init() { }

        protected virtual void Update() {
            if (IsBusy || Creature.CurrentState == Creature.CreatureState.Dead) return;

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
                case Creature.CreatureState.RunningAway:
                    RunAway();
                    break;
                case Creature.CreatureState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract void FollowPath();

        private void StayIdle() {
            IsBusy = true;
            CurrentSpeed = 0;
            StartCoroutine(StayIdleForSeconds(secondsToStayIdle));
        }

        protected virtual void Patrol() {
            IsBusy = true;
            CurrentSpeed = patrolSpeed;
        }

        protected virtual void RunAway() {
            IsBusy = true;
            CurrentSpeed = runSpeed;
        }

        public void CancelCurrentOrder() {
            IsBusy = false;
            HasMovingOrder = false;
        }

        private IEnumerator StayIdleForSeconds(float secondsToStayIdle) {
            yield return new WaitForSeconds(secondsToStayIdle);
            IsBusy = false;
        }

        public void OnDeath() {
            HasMovingOrder = false;
            IsBusy = false;
        }
    }
}