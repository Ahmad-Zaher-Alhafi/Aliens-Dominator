using System;
using System.Collections.Generic;
using System.Linq;
using Context;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Creatures {
    public class GroundCreatureMover : CreatureMover {
        private NavMeshAgent navMeshAgent;
        private NavMeshPath navMeshPath;
        private bool HasReachedDestination => navMeshAgent.remainingDistance <= stoppingDistance;

        protected override void Awake() {
            base.Awake();
            navMeshPath = new NavMeshPath();

            if (navMeshAgent == null) {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        public override void Init(SpawnPointPath pathToFollow) {
            base.Init(pathToFollow);
            navMeshAgent.enabled = true;
            navMeshAgent.speed = CurrentSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if ((CreatureStateType) Creature.CurrentStateType == CreatureStateType.Dead) {
                navMeshAgent.enabled = false;
                return;
            }

            navMeshAgent.speed = CurrentSpeed;
            if (HasMovingOrder) {
                if (HasReachedDestination) {
                    OnDestinationReached();
                }
            }
        }

        public override void Patrol(Action informOrderFulfilled) {
            base.Patrol(informOrderFulfilled);
            Transform patrolPoint = MathUtils.GetRandomObjectFromList(Ctx.Deps.CreatureSpawnController.GroundCinematicEnemyPathPoints).transform;
            OrderToMoveTo(patrolPoint);
        }

        public override void RunAway(Action informOrderFulfilled) {
            base.RunAway(informOrderFulfilled);
            Transform closestRunAwayPoint = FindClosestPoint(Ctx.Deps.CreatureSpawnController.RunningAwayPoints);
            OrderToMoveTo(closestRunAwayPoint);
        }

        public override PathPoint FollowPath(Action informOrderFulfilled) {
            Transform nextPathPoint = base.FollowPath(informOrderFulfilled)?.transform;
            if (nextPathPoint == null) return null;

            OrderToMoveTo(nextPathPoint);
            return null;
        }

        protected override void OrderToMoveTo(Transform point) {
            base.OrderToMoveTo(point);
            NavMesh.CalculatePath(transform.position, point.position, NavMesh.AllAreas, navMeshPath);
            navMeshAgent.SetPath(navMeshPath);
        }

        private Transform FindClosestPoint(IEnumerable<Transform> points) {
            return points.OrderBy(point => Vector3.Distance(transform.position, point.position)).First();
        }
    }
}