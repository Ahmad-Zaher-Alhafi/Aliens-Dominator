using System;
using System.Collections;
using System.Linq;
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

        // Distance between creature and destination point to stop moving
        [SerializeField] protected float stoppingDistance = 1;
        // Angel between creature and destination point to stop rotating
        [SerializeField] protected float stoppingAngel = 1;

        private float RotatingSpeed => rotatingSpeed;
        protected Creature Creature;
        protected bool HasMovingOrder;
        private SpawnPointPath pathToFollow;
        private int lastFollowedPathPointIndex = -1;
        private PathPoint lastFollowedPathPoint;
        private bool HasReachedPathEnd => pathToFollow.PathPoints.Count == 0 || pathToFollow.PathPoints.Last() == lastFollowedPathPoint;

        public bool HasReachedAttackPoint {
            get;
            private set;
        }

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
                case Creature.CreatureState.ChasingTarget:
                    ChaseTarget(Creature.AttackPoint.transform.position);
                    break;
                case Creature.CreatureState.Attacking:
                    RotateToTheWantedAngle(Creature.ObjectToAttack.transform.position);
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
            lastFollowedPathPoint = nextPathPoint;
            CurrentSpeed = runSpeed;
            if (nextPathPoint != null) return nextPathPoint;

            // Has Reached the end of the path
            OnDestinationReached();
            return nextPathPoint;
        }

        private void ChaseTarget(Vector3 targetPosition) {
            IsBusy = true;
            CurrentSpeed = runSpeed;
            OrderToMoveTo(targetPosition);
        }

        private void StayIdle() {
            IsBusy = true;
            CurrentSpeed = 0;
            StartCoroutine(StayIdleForSeconds(secondsToStayIdle));
        }

        protected virtual void OrderToMoveTo(Vector3 position) {
            HasMovingOrder = true;
        }

        protected virtual void Patrol() {
            IsBusy = true;
            CurrentSpeed = patrolSpeed;
        }

        protected virtual void RunAway() {
            IsBusy = true;
            CurrentSpeed = runSpeed;
        }

        /// <summary>
        /// To rotate the creature to the right direction
        /// </summary>
        /// <param name="targetPosition">Position where the creature look towards</param>
        protected void RotateToTheWantedAngle(Vector3 targetPosition) {
            float dot = Vector3.Dot(transform.forward, (targetPosition - transform.position).normalized);
            if(dot >= .9f) return;

            var creatureTransform = transform;
            Vector3 direction = targetPosition - creatureTransform.position;
            Vector3 newDirection = Vector3.RotateTowards(creatureTransform.forward, direction, RotatingSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        public void FulfillCurrentOrder() {
            Creature.OnOrderFulfilled();
            HasMovingOrder = false;
            IsBusy = false;
            CurrentSpeed = 0;
        }

        protected void OnDestinationReached() {
            if (Creature.CurrentState == Creature.CreatureState.FollowingPath && !HasReachedPathEnd) {
                FollowPath();
            } else {
                if (Creature.CurrentState == Creature.CreatureState.FollowingPath && Creature.AttackPoint != null) {
                    HasReachedAttackPoint = true;
                }

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
            lastFollowedPathPoint = null;
            HasReachedAttackPoint = false;
        }
    }
}