using System;
using System.Collections;
using System.Linq;
using FiniteStateMachine.States;
using UnityEngine;
using Utils;

namespace Creatures {
    public abstract class CreatureMover : MonoBehaviour {
        public bool IsBusy {
            get => isBusy;
            private set {
                isBusy = value;
                if (!isBusy) {
                    informOrderFulfilled?.Invoke();
                }
            }
        }
        private bool isBusy;

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
        private Action informOrderFulfilled;

        private bool HasReachedPathEnd => pathToFollow.PathPoints.Count == 0 || pathToFollow.PathPoints.Last() == lastFollowedPathPoint;

        public bool HasReachedBaseAttackPoint { get; private set; }

        protected virtual void Awake() {
            Creature = GetComponent<Creature>();
        }

        public virtual void Init(SpawnPointPath pathToFollow) {
            this.pathToFollow = pathToFollow;
        }

        protected virtual void Update() {
            if (IsBusy || Creature.CurrentState == StateType.Dead) return;

            if (Creature.CurrentState == StateType.Attacking) {
                RotateToTheWantedAngle(Creature.ObjectToAttack.transform.position);
            }
        }

        public void StayIdle(Action informOrderFulfilled) {
            IsBusy = true;
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = 0;
            StartCoroutine(StayIdleForSeconds(secondsToStayIdle));
        }

        public virtual void Patrol(Action informOrderFulfilled) {
            IsBusy = true;
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = patrolSpeed;
        }

        public virtual void RunAway(Action informOrderFulfilled) {
            IsBusy = true;
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = runSpeed;
        }

        public virtual PathPoint FollowPath(Action informOrderFulfilled) {
            IsBusy = true;
            this.informOrderFulfilled = informOrderFulfilled;
            return ContinueToNextPathPoint();
        }

        public void ChaseTarget(Action informOrderFulfilled, Vector3 targetPosition) {
            IsBusy = true;
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = runSpeed;
            OrderToMoveTo(targetPosition);
        }

        private PathPoint ContinueToNextPathPoint() {
            PathPoint nextPathPoint = MathUtils.GetNextObjectInList(pathToFollow.PathPoints, lastFollowedPathPointIndex);
            lastFollowedPathPointIndex = pathToFollow.PathPoints.IndexOf(nextPathPoint);
            lastFollowedPathPoint = nextPathPoint;
            CurrentSpeed = runSpeed;
            if (nextPathPoint != null) return nextPathPoint;

            // Has Reached the end of the path
            OnDestinationReached();
            return null;
        }

        protected virtual void OrderToMoveTo(Vector3 position) {
            HasMovingOrder = true;
        }

        /// <summary>
        /// To rotate the creature to the right direction
        /// </summary>
        /// <param name="targetPosition">Position where the creature look towards</param>
        protected void RotateToTheWantedAngle(Vector3 targetPosition) {
            float dot = Vector3.Dot(transform.forward, (targetPosition - transform.position).normalized);
            if (dot >= .9f) return;

            var creatureTransform = transform;
            Vector3 direction = targetPosition - creatureTransform.position;
            Vector3 newDirection = Vector3.RotateTowards(creatureTransform.forward, direction, RotatingSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        private void FulfillCurrentOrder() {
            TerminateCurrentOrder();
            Creature.OnMoverOrderFulfilled();
        }

        public void TerminateCurrentOrder() {
            HasMovingOrder = false;
            IsBusy = false;
            CurrentSpeed = 0;
        }

        protected void OnDestinationReached() {
            if (Creature.CurrentState == StateType.FollowingPath && !HasReachedPathEnd) {
                ContinueToNextPathPoint();
            } else {
                if (Creature.CurrentState == StateType.FollowingPath && Creature.TargetPoint != null) {
                    HasReachedBaseAttackPoint = true;
                }

                FulfillCurrentOrder();
            }
        }

        private IEnumerator StayIdleForSeconds(float secondsToStayIdle) {
            yield return new WaitForSeconds(secondsToStayIdle);
            FulfillCurrentOrder();
        }

        public void OnDeath() {
            lastFollowedPathPointIndex = -1;
            lastFollowedPathPoint = null;
            HasReachedBaseAttackPoint = false;
        }
    }
}