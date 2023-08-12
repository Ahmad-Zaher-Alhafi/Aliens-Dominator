using System;
using System.Collections;
using System.Linq;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;
using Utils;

namespace Creatures {
    public abstract class CreatureMover : MonoBehaviour {
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
        private Action<bool> informOrderFulfilled;

        private bool HasReachedPathEnd => pathToFollow.PathPoints.Count == 0 || pathToFollow.PathPoints.Last() == lastFollowedPathPoint;

        public bool HasReachedBaseAttackPoint { get; private set; }

        protected virtual void Awake() {
            Creature = GetComponent<Creature>();
        }

        public virtual void Init(SpawnPointPath pathToFollow) {
            this.pathToFollow = pathToFollow;
        }

        protected virtual void FixedUpdate() {
            if (Creature.CurrentStateType == CreatureStateType.Dead) return;

            if (Creature.CurrentStateType == CreatureStateType.Attacking) {
                RotateToTheWantedAngle(Creature.ObjectToAttack.transform.position);
            }
        }

        public void StayIdle(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = 0;
            StartCoroutine(StayIdleForSeconds(secondsToStayIdle));
        }

        public virtual void Patrol(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = patrolSpeed;
        }

        public virtual void RunAway(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = runSpeed;
        }

        public virtual PathPoint FollowPath(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            return ContinueToNextPathPoint();
        }

        public void ChaseTarget(Action<bool> informOrderFulfilled, Transform target) {
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = runSpeed;
            OrderToMoveTo(target);
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

        protected virtual void OrderToMoveTo(Transform point) {
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
            OnCurrentOrderFinished(false);
            Creature.OnMoverOrderFulfilled();
        }

        public void InterruptCurrentOrder() {
            OnCurrentOrderFinished(true);
        }

        private void OnCurrentOrderFinished(bool wasInterrupted) {
            HasMovingOrder = false;
            informOrderFulfilled?.Invoke(wasInterrupted);
            CurrentSpeed = 0;
        }

        protected void OnDestinationReached() {
            if (Creature.CurrentStateType == CreatureStateType.FollowingPath && !HasReachedPathEnd) {
                ContinueToNextPathPoint();
            } else {
                if (Creature.CurrentStateType == CreatureStateType.FollowingPath && Creature.TargetPoint != null) {
                    HasReachedBaseAttackPoint = true;
                }

                FulfillCurrentOrder();
            }
        }

        private IEnumerator StayIdleForSeconds(float secondsToStayIdle) {
            yield return new WaitForSeconds(secondsToStayIdle);
            if (Creature.CurrentStateType == CreatureStateType.Idle) {
                FulfillCurrentOrder();
            }
        }

        public void OnDeath() {
            lastFollowedPathPointIndex = -1;
            lastFollowedPathPoint = null;
            HasReachedBaseAttackPoint = false;
        }
    }
}