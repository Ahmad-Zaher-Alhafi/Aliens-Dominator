using ManagersAndControllers;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Creatures {
    public class FlyingCreatureMover : CreatureMover {
        private Vector3 wantedAngle; // Wanted angle is the angle that the creature has to rotate to for reaching the wanted point
        private Vector3 current;
        private CreatureSpawnController creatureSpawnController;
        private Vector3 positionToMoveTo;

        protected override void Awake() {
            base.Awake();
            creatureSpawnController = FindObjectOfType<CreatureSpawnController>();
        }

        protected override void Update() {
            base.Update();
            if (HasMovingOrder) {
                MoveTo(positionToMoveTo);
                RotateToTheWantedAngle(positionToMoveTo);
            }
        }

        protected override void Patrol() {
            base.Patrol();
            Transform nextCinematicPatrolPoint = MathUtils.GetRandomObjectFromList(creatureSpawnController.AirCinematicEnemyPathPoints).transform;
            OrderToMove(nextCinematicPatrolPoint.position);
        }

        protected override void RunAway() {
            base.RunAway();
            Transform randomRunAwayPoint = creatureSpawnController.RunningAwayPoints[Random.Range(0, creatureSpawnController.RunningAwayPoints.Count)].transform;
            OrderToMove(randomRunAwayPoint.position);
        }

        protected override PathPoint FollowPath() {
            Transform nextPathPoint = base.FollowPath()?.transform;
            if (nextPathPoint == null) return null;
            
            OrderToMove(nextPathPoint.position);
            RotateToTheWantedAngle(nextPathPoint.transform.position);
            return null;
        }

        /// <summary>
        /// To rotate the creature to the right direction
        /// </summary>
        /// <param name="targetPosition">Position where the creature look towards</param>
        private void RotateToTheWantedAngle(Vector3 targetPosition) {
            var creatureTransform = transform;
            Vector3 direction = targetPosition - creatureTransform.position;
            Vector3 newDirection = Vector3.RotateTowards(creatureTransform.forward, direction, RotatingSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        private void OrderToMove(Vector3 position) {
            HasMovingOrder = true;
            positionToMoveTo = position;
        }

        private void MoveTo(Vector3 position) {
            // If the creature has not reached the position
            if (Vector3.Distance(transform.position, position) >= 1) {
                transform.position = Vector3.MoveTowards(transform.position, position, CurrentSpeed * Time.deltaTime);
            } else {
                OnDestinationReached();
            }
        }
    }
}