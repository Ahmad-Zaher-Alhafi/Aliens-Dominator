using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using ManagersAndControllers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Creatures {
    public class GroundCreatureMover : CreatureMover {
        [SerializeField]
        public bool _patrolWaiting;

        [SerializeField]
        private float _totalWaitTime;
        [SerializeField]
        private float wayPointsRemainedDistance = 1; //the distance between the creature and the waypoint that he is going to which allow him to go to the next point
        [SerializeField]
        private float _switchProbability;

        [HideInInspector]
        public bool isDamaging;
        [HideInInspector]
        public NavMeshAgent NavMeshAgent;
        public int CurrentPatrolIndex;
        [HideInInspector]
        public string EnemyId;
        [FormerlySerializedAs("Spawner")]
        [HideInInspector]
        public CreatureSpawnController creatureSpawnController;

        public string WalkAnimationParameter;

        public int CinematicEnemiesPath = 9;
        [SerializeField] private float TimeToWaitForAnimation = 3f;
        [SerializeField] private float TimeToRoar = 1f;

        [SerializeField] private float MaxSpeed = 10f;
        [SerializeField] private float HypnotizedSpeedBonus = 1f;

        [Header("Rotating towards wall")]
        public float SmoothRotating = 8f;

        public Waypoint EndPoint;

        [HideInInspector]
        public string TrackEnemyID;
        private bool _travelling;
        private bool _waiting;
        private float _waitTimer;
        private Animator animator;
        private float ChangeSpeedTimer;

        private float ChangeTimeMax;

        private bool CinematicEnemyHit;

        private Coroutine Coroutine;
        //public string AttackAnimationParameter;


        private int currentPath;

        private float DistanceFromWaypoint;

        private IEnumerator FighterRoutine;

        private GameHandler GameHandler;
        private bool isDefending;
        public float speedDivider = 2f;

        public float TotalShootTime = 10f;
        public float TotalDiggingTime = 5f;
        public float TotalTeleportTime = 5f;
        public int ChanceToMakeSpecialAction;
        /// <summary>
        ///     Only applies for start enemies, so they stop moving or creating paths while playing an animation
        /// </summary>
        private bool IsExecutingAnimation;

        private bool IsHypnotized;
        private float NewSpeed;
        private NavMeshPath Path;

        private bool Stop;
        private float Timer;

        private readonly float TimeUntilNextHit = 1f;
        private float UpdatePathTimer;
        private bool wasDigging;
        private bool wasSpawnningBugs;
        private bool wasSpinning;
        private bool wasTeleporting;
        private Coroutine stayingIdleRoutine;
        private bool HasReachedDestination => NavMeshAgent.remainingDistance <= wayPointsRemainedDistance;
        private GameObject EnemyToFollow {
            get {
                if (TrackEnemyID == null) return null;



                return null;
            }
        }

        protected override void Awake() {
            base.Awake();
            TrackEnemyID = null;

            Path = new NavMeshPath();

            GameHandler = FindObjectOfType<GameHandler>();


            animator = GetComponent<Animator>();

            creatureSpawnController = FindObjectOfType<CreatureSpawnController>();


            //FollowRandomPath();

            //You see it right, I'm using events haha
            //This will fire once one enemy got killed/ hit, so enemies stop running in circles
            Ctx.Deps.EventsManager.WaveStarted += CinematicEnemyHitEvent;
        }

        public override void Init() {
            base.Init();
            if (NavMeshAgent == null) {
                NavMeshAgent = GetComponent<NavMeshAgent>();
            }

            NavMeshAgent.enabled = true;
            NavMeshAgent.speed = CurrentSpeed;
            NavMeshAgent.stoppingDistance = wayPointsRemainedDistance;
        }

        protected override void Update() {
            base.Update();
            if (Creature.CurrentState == Creature.CreatureState.Dead) {
                NavMeshAgent.enabled = false;
                return;
            }

            NavMeshAgent.speed = CurrentSpeed;
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
                GetRandomObjectFromList(creatureSpawnController.GroundCinematicEnemyWaypoints).transform.position, NavMesh.AllAreas, Path);
            NavMeshAgent.SetPath(Path);
        }

        private void OnDestinationReached() {
            IsBusy = false;
            HasMovingOrder = false;
            NavMeshAgent.speed = 0;
        }

        protected override void RunAway() {
            base.RunAway();
            HasMovingOrder = true;
            Transform closestRunAwayPoint = FindClosestPoint(creatureSpawnController.RunningAwayPoints);
            NavMesh.CalculatePath(transform.position, closestRunAwayPoint.position, NavMesh.AllAreas, Path);
            NavMeshAgent.SetPath(Path);
        }

        protected override void FollowPath() {
            NavMeshAgent.speed = CurrentSpeed;
            List<Waypoint> waypoints = creatureSpawnController.GroundCinematicEnemyWaypoints;

            if (NavMeshAgent.remainingDistance <= wayPointsRemainedDistance && waypoints.Count > 0) {
                GetRandomObjectFromList(creatureSpawnController.GroundCinematicEnemyWaypoints);
                NavMesh.CalculatePath(transform.position,
                    GetRandomObjectFromList(creatureSpawnController.GroundCinematicEnemyWaypoints).transform.position, NavMesh.AllAreas, Path);
                NavMeshAgent.SetPath(Path);
            } else {
                IsBusy = false;
            }
        }

        private Transform FindClosestPoint(List<Transform> points) {
            return points.OrderBy(point => Vector3.Distance(transform.position, point.position)).First();
        }

        private void OnEnable() {
            EnemyId = null;
            wasDigging = false;
            wasSpinning = false;
            wasTeleporting = false;
            wasSpawnningBugs = false;

            isDamaging = false;
            Stop = false;

            CurrentPatrolIndex = 0;
            _patrolWaiting = false;
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        public void OnDestroy() {
            Ctx.Deps.EventsManager.WaveStarted -= CinematicEnemyHitEvent;
        }

        public T GetRandomObjectFromList<T>(IReadOnlyList<T> waypointsList) {
            int randomNumber = Random.Range(0, waypointsList.Count + 1);
            randomNumber = Mathf.Clamp(randomNumber, 0, waypointsList.Count - 1);
            T randomPoint = waypointsList[randomNumber];
            return randomPoint;
        }

        public void SetCreatureSpeed(float speed) {
            NavMeshAgent.speed = speed;
        }

        private bool MakeSpecialAction() {
            int chance = ChanceToMakeSpecialAction;
            int selectedNum = Random.Range(1, 101);

            if (selectedNum <= chance) return true;

            return false;
        }

        private IEnumerator EnableShootingForEnemy() {
            if (EndPoint || Stop || Creature.CurrentState == Creature.CreatureState.Dead) {
                StopCoroutine(FighterRoutine);
                yield break;
            }

            //Stop moving and init shooting
            NavMeshAgent.enabled = false;

            yield return new WaitForSeconds(TotalShootTime);

            //If creature not dead and not at wall, continue moving
            if (!NavMeshAgent.enabled && Creature.CurrentState != Creature.CreatureState.Dead && !EndPoint && !Stop) {
                //Creature.OrderToStopShooting();

                NavMeshAgent.enabled = true;

                SetGroundedEnemyWaypoint();
                NavMeshAgent.speed = CurrentSpeed;

                StopCoroutine(FighterRoutine);
            } else {
                //Creature.OrderToStopShooting();
                StopCoroutine(FighterRoutine);
            }
        }

        private IEnumerator EnableDigging() {
            wasDigging = true;
            var digger = GetComponent<DiggingCreature>();

            yield return new WaitForSeconds(TotalDiggingTime);

            wasDigging = false;
            ++CurrentPatrolIndex;

            NavMeshAgent.enabled = true;

            SetGroundedEnemyWaypoint();
            NavMeshAgent.speed = CurrentSpeed;
        }

        private IEnumerator Teleport() {
            wasTeleporting = true;
            NavMeshAgent.enabled = false;

            yield return new WaitForSeconds(TotalTeleportTime);

            wasTeleporting = false;
            ++CurrentPatrolIndex;

            NavMeshAgent.enabled = true;

            SetGroundedEnemyWaypoint();
            NavMeshAgent.speed = CurrentSpeed;
        }

        //Goes to the attack spot
        private void SetEndPoint(Waypoint newPos) {
            if (NavMesh.CalculatePath(transform.position, newPos.gameObject.transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
        }

        //Sets path of ground enemies
        public void SetGroundedEnemyWaypoint() {
            if (!NavMeshAgent || !NavMeshAgent.enabled || Creature.CurrentState == Creature.CreatureState.Dead) return;
            NavMeshAgent.SetPath(Path);
        }

        //Sets destroy path of cinematic enemies once one of them was hit
        private void CinematicEnemyHitEvent() {
            if (CompareTag("OnStartWaves")) StartCoroutine(WaitForStartCreatureToFinishAnimation(TimeToRoar, Constants.AnimationsTypes.Roar, true));
        }

        //Attack enemy if not null and in a certain time interval
        private void FollowEnemyWhileHypnotized() {
            GameObject enemy = EnemyToFollow;

            if (!enemy) {
                GetHypnotized(true);
                return;
            }

            if (Timer >= TimeUntilNextHit) {
                animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Attack));

                NavMeshAgent.speed = 0.3f;

                //enemy.GetComponent<Creature>().ReceiveDamageFromEnemy(gameObject, Creature.AttackDamage, 100f);

                Timer = 0f;

            }
        }

        private void Defense() {
            GameObject enemy = EnemyToFollow;

            if (!enemy) {
                NavMeshAgent.speed = CurrentSpeed;
                TrackEnemyID = null;
                isDefending = false;

                SetGroundedEnemyWaypoint();
                return;
            }

            if (Timer >= TimeUntilNextHit) {
                animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Attack));
                //enemy.GetComponent<Creature>().ReceiveDamageFromEnemy(gameObject, Creature.AttackDamage, 100f);

                Timer = 0f;
            }
        }



        private IEnumerator WaitForStartCreatureToFinishAnimation(float time, Constants.AnimationsTypes anim, bool cinematicEnemyHit = false) {
            IsExecutingAnimation = true;

            NavMeshAgent.speed = 0f;

            StartMovingAnimation();

            animator.Play(Constants.GetAnimationName(gameObject.name, anim));

            yield return new WaitForSeconds(time);

            if (cinematicEnemyHit) {
                CinematicEnemyHit = true;
                NavMeshAgent.speed = MaxSpeed * 2;

                //Update run animation
                StartMovingAnimation();

                if (NavMesh.CalculatePath(transform.position, creatureSpawnController.RunningAwayPoints[Random.Range(0, creatureSpawnController.RunningAwayPoints.Count)].position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
            } else {
                List<Waypoint> waypoints = creatureSpawnController.GroundCinematicEnemyWaypoints;

                NavMeshAgent.speed = CurrentSpeed;

                if (NavMesh.CalculatePath(transform.position, GetRandomObjectFromList(creatureSpawnController.GroundCinematicEnemyWaypoints).transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);

                StartMovingAnimation();

                IsExecutingAnimation = false;
            }
        }

        /// <summary>
        ///     to activate the animator and start moving animation
        /// </summary>
        public void StartMovingAnimation() {
            animator.SetFloat("WalkSpeed", NavMeshAgent.speed / MaxSpeed);
        }

        //Find enemy and start decreasing your health
        private void FollowEnemy() {
            GameObject enemyToTrack = EnemyToFollow;
            var creature = enemyToTrack.GetComponent<Creature>();

            if (enemyToTrack && creature.enabled) {
                //Must be set to false, so this enemy walks away from the wall if it is currently attacking the wall
                if (Stop) {
                    Stop = false;
                    StopCoroutine(Coroutine);
                }

                NavMeshAgent.enabled = true;

                //StartCoroutine(DecreaseHealth());

                NavMeshAgent.speed = CurrentSpeed + HypnotizedSpeedBonus;

                if (NavMesh.CalculatePath(transform.position, enemyToTrack.transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
            } else if (!creature) {
                GetHypnotized();
            }
        }

        public void EndRoutine() {
            if (Coroutine == null) return;

            StopCoroutine(Coroutine);
        }

        /// <summary>
        ///     Gets hit by hypnotized arrow and find closest enemy
        /// </summary>
        public void GetHypnotized(bool findNewEnemy = false) {
            if (IsHypnotized && !findNewEnemy) return;

            List<GameObject> fixedList = GameHandler.AllEnemies.FindAll(e => {
                var creature = e.GetComponent<Creature>();

                if (!creature || !creature.enabled) return false;


                if (creature is GroundCreature) return false;

                return true;
            });

            if (fixedList.Count <= 0) {
                IsHypnotized = false;

                return;
            }

            fixedList.Sort((e1, e2) => {
                float distance1 = Vector3.Distance(transform.position, e1.gameObject.transform.position);
                float distance2 = Vector3.Distance(transform.position, e2.gameObject.transform.position);

                return distance1.CompareTo(distance2);
            });

            IsHypnotized = true;
            isDamaging = false;

            FollowEnemy();
        }

        //Set creature properties, so only one enemy gets assigned at a time
        public void FocusOnAttacker(string enemyId) {
            Stop = false;
            EndRoutine();

            TrackEnemyID = enemyId;
            isDefending = true;
            isDamaging = false;

            NavMeshAgent.speed = 0.3f;

            if (NavMesh.CalculatePath(transform.position, EnemyToFollow.transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
        }

        private IEnumerator SlowDown() {
            GameObject[] enemies = GameHandler.AllEnemies.ToArray();

            foreach (GameObject e in enemies)
                if (e) {
                    var agent = e.GetComponent<NavMeshAgent>();
                    var creature = e.GetComponent<Creature>();

                    // If the creature is not slowed down 
                    if (!creature.IsSlowedDown) {
                        // If it was a ground creature so it has a nave mesh agent
                        if (agent != null) {

                            agent.speed /= speedDivider;
                        } else { //if it was air creature

                            creature.GetComponent<FlyingCreature>().SetPatrollingSpeed(speedDivider, true);
                        }


                    }
                }

            yield return new WaitForSeconds(1);
            foreach (GameObject e in enemies) {
                if (e) {
                    var agent = e.GetComponent<NavMeshAgent>();
                    var creature = e.GetComponent<Creatures.Creature>();

                    // If the creature is slowed down 
                    if (creature.IsSlowedDown) {
                        //if it was a ground creature so it has a nave mesh agent
                        if (agent != null) {

                            agent.speed *= speedDivider;
                        } else { //if it was air creature

                            creature.GetComponent<FlyingCreature>().SetPatrollingSpeed(speedDivider, false);
                        }
                    }
                }
            }
        }

        private IEnumerator UpdateCreatureSpeed() {
            yield return new WaitForSeconds(.5f);
        }
    }
}