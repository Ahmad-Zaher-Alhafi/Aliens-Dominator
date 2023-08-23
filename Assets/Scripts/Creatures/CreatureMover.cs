using System;
using System.Collections;
using System.Linq;
using Context;
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
        public PathPoint LastReachedPathPoint { get; private set; }
        private PathPoint pathPointCurrentlyGoingTo;
        private Action<bool> informOrderFulfilled;

        public bool HasReachedPathEnd => pathToFollow.PathPoints.Count == 0 || pathToFollow.PathPoints.Last() == LastReachedPathPoint;

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

        public void FollowPath(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            ContinueToNextPathPoint();
        }

        public void ChaseTarget(Action<bool> informOrderFulfilled, Transform target) {
            this.informOrderFulfilled = informOrderFulfilled;
            CurrentSpeed = runSpeed;
            OrderToMoveTo(target);
        }

        private void ContinueToNextPathPoint() {
            PathPoint nextPathPoint = MathUtils.GetNextObjectInList(pathToFollow.PathPoints, LastReachedPathPoint != null ? LastReachedPathPoint.Index : -1);
            pathPointCurrentlyGoingTo = nextPathPoint;
            CurrentSpeed = runSpeed;

            if (nextPathPoint != null) {
                OrderToMoveTo(nextPathPoint.transform);
                return;
            }

            // Has Reached the end of the path
            OnDestinationReached();
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
            if (Creature.CurrentStateType == CreatureStateType.FollowingPath) {
                LastReachedPathPoint = pathPointCurrentlyGoingTo;
                Ctx.Deps.EventsManager.TriggerPathPointReached(LastReachedPathPoint);
                if (!HasReachedPathEnd) {
                    ContinueToNextPathPoint();
                    return;
                }
            }

            FulfillCurrentOrder();
        }

        private IEnumerator StayIdleForSeconds(float secondsToStayIdle) {
            yield return new WaitForSeconds(secondsToStayIdle);
            if (Creature.CurrentStateType == CreatureStateType.Idle) {
                FulfillCurrentOrder();
            }
        }

        public void OnDeath() {
            LastReachedPathPoint = null;
            pathPointCurrentlyGoingTo = null;
        }
    }
}