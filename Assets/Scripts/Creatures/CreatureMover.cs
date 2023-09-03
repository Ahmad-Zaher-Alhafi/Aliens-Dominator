using System;
using System.Collections;
using System.Linq;
using Context;
using UnityEngine;
using Utils;

namespace Creatures {
    public abstract class CreatureMover : MonoBehaviour {
        [SerializeField] private float secondsToStayIdle = 5;
        // Distance between creature and destination point to stop moving
        [SerializeField] protected float stoppingDistance = 1;
        // Angel between creature and destination point to stop rotating
        [SerializeField] protected float stoppingAngel = 1;

        protected Creature Creature;
        protected bool HasMovingOrder;
        private SpawnPointPath pathToFollow;
        public PathPoint LastReachedPathPoint { get; private set; }
        private PathPoint pathPointCurrentlyGoingTo;
        private Action<bool> informOrderFulfilled;

        public bool HasReachedPathEnd {
            get {
                if (pathToFollow == null) return true;
                return pathToFollow.PathPoints.Count == 0 || pathToFollow.PathPoints.Last() == LastReachedPathPoint;
            }
        }

        protected virtual void Awake() {
            Creature = GetComponent<Creature>();
        }

        public virtual void Init(SpawnPointPath pathToFollow) {
            this.pathToFollow = pathToFollow;
        }

        protected virtual void FixedUpdate() {
            if (Creature.IsDead) return;

            if (Creature.IsAttacking) {
                RotateToTheWantedAngle(Creature.ObjectToAttack.transform.position);
            }
        }

        public void StayIdle(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            StartCoroutine(StayIdleForSeconds(secondsToStayIdle));
        }

        public virtual void Patrol(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
        }

        public virtual void RunAway(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
        }

        public void FollowPath(Action<bool> informOrderFulfilled) {
            this.informOrderFulfilled = informOrderFulfilled;
            ContinueToNextPathPoint();
        }

        public void ChaseTarget(Action<bool> informOrderFulfilled, Transform target) {
            this.informOrderFulfilled = informOrderFulfilled;
            OrderToMoveTo(target);
        }

        private void ContinueToNextPathPoint() {
            PathPoint nextPathPoint = MathUtils.GetNextObjectInList(pathToFollow.PathPoints, LastReachedPathPoint != null ? LastReachedPathPoint.Index : -1);
            pathPointCurrentlyGoingTo = nextPathPoint;

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
            Vector3 newDirection = Vector3.RotateTowards(creatureTransform.forward, direction, Creature.RotatingSpeed * Time.deltaTime, 0);
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
        }

        protected void OnDestinationReached() {
            if (Creature.IsFollowingPath) {
                LastReachedPathPoint = pathPointCurrentlyGoingTo;
                Ctx.Deps.EventsManager.TriggerPathPointReached(Creature, LastReachedPathPoint);
                if (!HasReachedPathEnd) {
                    ContinueToNextPathPoint();
                    return;
                }
            }

            FulfillCurrentOrder();
        }

        private IEnumerator StayIdleForSeconds(float secondsToStayIdle) {
            yield return new WaitForSeconds(secondsToStayIdle);
            if (Creature.IsIdle) {
                FulfillCurrentOrder();
            }
        }

        public void OnDeath() {
            LastReachedPathPoint = null;
            pathPointCurrentlyGoingTo = null;
        }
    }
}