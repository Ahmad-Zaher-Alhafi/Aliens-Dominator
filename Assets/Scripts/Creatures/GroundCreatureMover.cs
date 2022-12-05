using System.Collections.Generic;
using System.Linq;
using Context;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Creatures {
    public class GroundCreatureMover : CreatureMover {
        // Distance between creature and destination point to stop
        [SerializeField] private float stoppingDistance = 1;

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

        protected override void Update() {
            base.Update();
            if (Creature.CurrentState == Creature.CreatureState.Dead) {
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

        protected override void Patrol() {
            base.Patrol();
            HasMovingOrder = true;
            NavMesh.CalculatePath(transform.position,
                MathUtils.GetRandomObjectFromList(Ctx.Deps.CreatureSpawnController.GroundCinematicEnemyPathPoints).transform.position, NavMesh.AllAreas, navMeshPath);
            navMeshAgent.SetPath(navMeshPath);
        }

        protected override void RunAway() {
            base.RunAway();
            HasMovingOrder = true;
            Transform closestRunAwayPoint = FindClosestPoint(Ctx.Deps.CreatureSpawnController.RunningAwayPoints);
            NavMesh.CalculatePath(transform.position, closestRunAwayPoint.position, NavMesh.AllAreas, navMeshPath);
            navMeshAgent.SetPath(navMeshPath);
        }

        protected override PathPoint FollowPath() {
            Transform nextPathPoint = base.FollowPath()?.transform;
            if (nextPathPoint == null) return null;

            NavMesh.CalculatePath(transform.position, nextPathPoint.position, NavMesh.AllAreas, navMeshPath);
            navMeshAgent.SetPath(navMeshPath);
            return null;
        }

        private Transform FindClosestPoint(IEnumerable<Transform> points) {
            return points.OrderBy(point => Vector3.Distance(transform.position, point.position)).First();
        }
    }
}