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
        /// <summary>
        /// This offset is needed because the center of the creature is offset by the animations on y-axis, this offset will allow the creature to move to points correctly according to his center
        /// </summary>
        private readonly Vector3 animationBugOffset = Vector3.down * 2.5f;

        protected override void Awake() {
            base.Awake();
            creatureSpawnController = FindObjectOfType<CreatureSpawnController>();
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (HasMovingOrder && pointToMoveTo != null) {
                MoveTo(pointToMoveTo.position + animationBugOffset);
                RotateToTheWantedAngle(pointToMoveTo.position + animationBugOffset);
            }
        }

        public override void Patrol(Action<bool> informOrderFulfilled) {
            base.Patrol(informOrderFulfilled);
            Transform nextCinematicPatrolPoint = MathUtils.GetRandomObjectFromList(creatureSpawnController.AirCinematicEnemyPathPoints).transform;
            OrderToMoveTo(nextCinematicPatrolPoint);
        }

        public override void RunAway(Action<bool> informOrderFulfilled) {
            base.RunAway(informOrderFulfilled);
            Transform randomRunAwayPoint = creatureSpawnController.RunningAwayPoints[Random.Range(0, creatureSpawnController.RunningAwayPoints.Count)].transform;
            OrderToMoveTo(randomRunAwayPoint);
        }

        protected override void OrderToMoveTo(Transform point) {
            base.OrderToMoveTo(point);
            pointToMoveTo = point;
        }

        private void MoveTo(Vector3 position) {
            // If the creature has not reached the position
            if (Vector3.Distance(transform.position, position) >= stoppingDistance) {
                transform.position = Vector3.MoveTowards(transform.position, position, Creature.CurrentSpeed * Time.deltaTime);
            } else {
                OnDestinationReached();
            }
        }
    }
}