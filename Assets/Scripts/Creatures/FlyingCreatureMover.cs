using System;
using ManagersAndControllers;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Creatures {
    public class FlyingCreatureMover : CreatureMover {
        private Vector3 wantedAngle; // Wanted angle is the angle that the creature has to rotate to for reaching the wanted point
        private Vector3 current;
        private CreatureSpawnController creatureSpawnController;
        private Transform pointToMoveTo;

        protected override void Awake() {
            base.Awake();
            creatureSpawnController = FindObjectOfType<CreatureSpawnController>();
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (HasMovingOrder) {
                MoveTo(pointToMoveTo.position);
                RotateToTheWantedAngle(pointToMoveTo.position);
            }
        }

        public override void Patrol(Action informOrderFulfilled) {
            base.Patrol(informOrderFulfilled);
            Transform nextCinematicPatrolPoint = MathUtils.GetRandomObjectFromList(creatureSpawnController.AirCinematicEnemyPathPoints).transform;
            OrderToMoveTo(nextCinematicPatrolPoint);
        }

        public override void RunAway(Action informOrderFulfilled) {
            base.RunAway(informOrderFulfilled);
            Transform randomRunAwayPoint = creatureSpawnController.RunningAwayPoints[Random.Range(0, creatureSpawnController.RunningAwayPoints.Count)].transform;
            OrderToMoveTo(randomRunAwayPoint);
        }

        public override PathPoint FollowPath(Action informOrderFulfilled) {
            Transform nextPathPoint = base.FollowPath(informOrderFulfilled)?.transform;
            if (nextPathPoint == null) return null;
            
            OrderToMoveTo(nextPathPoint);
            return null;
        }

        protected override void OrderToMoveTo(Transform point) {
            base.OrderToMoveTo(point);
            pointToMoveTo = point;
        }

        private void MoveTo(Vector3 position) {
            // If the creature has not reached the position
            if (Vector3.Distance(transform.position, position) >= stoppingDistance) {
                transform.position = Vector3.MoveTowards(transform.position, position, CurrentSpeed * Time.deltaTime);
            } else {
                OnDestinationReached();
            }
        }
    }
}