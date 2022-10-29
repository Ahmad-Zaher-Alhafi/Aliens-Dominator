using System.Collections.Generic;
using System.Linq;
using ManagersAndControllers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Creatures {
    public class FlyingCreatureMover : CreatureMover {
        private Vector3 wantedAngle; // Wanted angle is the angle that the creature has to rotate to for reaching the wanted point
        private Vector3 current;
        private Transform nextCinematicPatrolPoint;
        private List<waypoint> airWayPoints = new(); // Air points which the creature has to follow
        private Spawner spawner;
        private Vector3 positionToMoveTo;


        protected override void Awake() {
            base.Awake();
            spawner = FindObjectOfType<Spawner>();
            airWayPoints = spawner.AirCinematicEnemyWaypoints.ToList();
            nextCinematicPatrolPoint = airWayPoints[Random.Range(0, airWayPoints.Count)].transform; //get a random patrol air point
        }

        protected override void Update() {
            base.Update();
            if (HasMovingOrder) {
                MoveTo(positionToMoveTo);
                RotateToTheWantedAngle(positionToMoveTo);
            }
        }

        protected override void Patrol() {
            nextCinematicPatrolPoint = airWayPoints[Random.Range(0, airWayPoints.Count)].transform;
            OrderToMove(nextCinematicPatrolPoint.position);
        }

        protected override void FollowPath() {
            nextCinematicPatrolPoint = airWayPoints[Random.Range(0, airWayPoints.Count)].transform;
            OrderToMove(nextCinematicPatrolPoint.position);
            //RotateToTheWantedAngle(nextCinematicPatrolPoint.transform.position);
        }

        /// <summary>
        /// To put the creature in the right rotation
        /// </summary>
        /// <param name="targetPosition">Position where the creature look towards</param>
        private void RotateToTheWantedAngle(Vector3 targetPosition) {
            var creatureTransform = transform;
            Vector3 direction = targetPosition - creatureTransform.position;
            Vector3 newDirection = Vector3.RotateTowards(creatureTransform.forward, direction, RotatingSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        private void OrderToMove(Vector3 position) {
            IsBusy = true;
            HasMovingOrder = true;
            positionToMoveTo = position;
        }

        private void MoveTo(Vector3 position) {
            // If the creature has not reached the position
            if (Vector3.Distance(transform.position, position) >= 1) {
                transform.position = Vector3.MoveTowards(transform.position, position, Speed * Time.deltaTime);
            } else {
                HasMovingOrder = false;
                IsBusy = false;
            }
        }
    }
}