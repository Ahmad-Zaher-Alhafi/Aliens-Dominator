using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Creatures {
    public abstract class CreatureMover : MonoBehaviour {
        public bool IsBusy { get; private set; }
        public float CurrentSpeed { get; private set; }

        [SerializeField] private float secondsToStayIdle = 5;
        [SerializeField] private float patrolSpeed = 3;
        public float PatrolSpeed => patrolSpeed;
        [SerializeField] private float runSpeed = 6;
        [SerializeField] private float rotatingSpeed = 1;

        protected float RotatingSpeed => rotatingSpeed;
        protected Creature Creature;
        protected bool HasMovingOrder;
        private SpawnPointPath pathToFollow;
        private int lastFollowedPathPointIndex = -1;
        private bool pathWasFinished;

        private bool HasToKeepFollowingPath => Creature.CurrentState == Creature.CreatureState.FollowingPath && !pathWasFinished;

        protected virtual void Awake() {
            Creature = GetComponent<Creature>();
        }

        public virtual void Init(SpawnPointPath pathToFollow) {
            this.pathToFollow = pathToFollow;
        }

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
                    FollowPath();
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

        protected virtual PathPoint FollowPath() {
            IsBusy = true;
            PathPoint nextPathPoint = MathUtils.GetNextObjectInList(pathToFollow.PathPoints, lastFollowedPathPointIndex);
            lastFollowedPathPointIndex = pathToFollow.PathPoints.IndexOf(nextPathPoint);
            CurrentSpeed = runSpeed;
            if (nextPathPoint != null) return nextPathPoint;

            // Has Reached the end of the path
            pathWasFinished = true;
            FulfillCurrentOrder();
            return nextPathPoint;
        }

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

        public void FulfillCurrentOrder() {
            Creature.OnOrderFulfilled();
            HasMovingOrder = false;
            IsBusy = false;
        }

        protected void OnDestinationReached() {
            if (HasToKeepFollowingPath) {
                FollowPath();
            } else {
                FulfillCurrentOrder();
            }
        }

        private IEnumerator StayIdleForSeconds(float secondsToStayIdle) {
            yield return new WaitForSeconds(secondsToStayIdle);
            FulfillCurrentOrder();
        }

        public void OnDeath() {
            FulfillCurrentOrder();
            lastFollowedPathPointIndex = -1;
            pathWasFinished = false;
        }
    }
}