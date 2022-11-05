using System;
using System.Collections;
using System.Collections.Generic;
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
        public List<Paths> PatrolPoints = new();

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

        public waypoint EndPoint;

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

                GameObject go = GameHandler.AllEnemies.FindAll(enemy => enemy.GetComponent<Creature>()).Find(enemy => enemy.GetComponent<Creature>().EnemyId == TrackEnemyID);
                if (!go) return null;

                return go;
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
            EventsManager.onStartEnemyDeath += CinematicEnemyHitEvent;

            if (NavMeshAgent) {
                if (PatrolPoints.Count <= 0) return;

                if (PatrolPoints[currentPath].Waypoints.Count > 0 && PatrolPoints[currentPath].Waypoints.Count >= 2) CurrentPatrolIndex = 0;
                else Debug.LogError("Insufficient patrol points for basic patrolling behaviour");
            }
        }

        public override void Init() {
            base.Init();
            if (NavMeshAgent == null) {
                NavMeshAgent = GetComponent<NavMeshAgent>();
            }
            
            NavMeshAgent.enabled = true;
            NavMeshAgent.speed = Speed;
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
                    IsBusy = false;
                    HasMovingOrder = false;
                    NavMeshAgent.speed = 0;
                }
            }

            /*if (!CompareTag("OnStartWaves")) {
                StartMovingAnimation();

                //Dont move anymore when attacking wall
                if (Stop || Creature.CurrentState == Creature.CreatureState.Dead) return;

                if (!NavMeshAgent.enabled) return;

                if (Timer <= TimeUntilNextHit) Timer += Time.deltaTime;

                DistanceFromWaypoint = NavMeshAgent.remainingDistance;

                if (IsHypnotized && DistanceFromWaypoint <= 0.3f) {
                    FollowEnemyWhileHypnotized();
                    return;
                }
                if (IsHypnotized && DistanceFromWaypoint > 0.3f) {
                    NavMeshAgent.speed = Speed + HypnotizedSpeedBonus;

                    GameObject go = EnemyToFollow;

                    if (!go) {
                        GetHypnotized();
                        return;
                    }

                    if (UpdatePathTimer >= 0.5f) {
                        if (NavMesh.CalculatePath(transform.position, go.transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);

                        UpdatePathTimer = 0f;
                    }

                    if (UpdatePathTimer <= 0.5f) UpdatePathTimer += Time.deltaTime;

                    return;
                }

                //Reached waypoint
                if (DistanceFromWaypoint <= 0.5f && !isDamaging && !isDefending) {
                    WaypointReached();
                    return;
                }

                //Reached attack point
                if (DistanceFromWaypoint <= 0.2f && isDamaging && !isDefending) {
                    NavMeshAgent.speed = 0f;
                    NavMeshAgent.enabled = false;

                    //Coroutine = StartCoroutine(AttackWall());

                    //Stop movement
                    Stop = true;

                    return;
                }

                if (isDefending) Defense();
            } else if (CompareTag("OnStartWaves") && !CinematicEnemyHit) {
                UpdateCinematicEnemyPosition();
            } else if (CompareTag("OnStartWaves") && CinematicEnemyHit) {
                if (NavMeshAgent.remainingDistance <= 0.5f) Destroy(gameObject);
            }*/
        }

        protected override void Patrol() {
            base.Patrol();
            HasMovingOrder = true;
            NavMesh.CalculatePath(transform.position, GetRandomWaypointForCinematicEnemy().transform.position, NavMesh.AllAreas, Path);
            NavMeshAgent.SetPath(Path);
        }

        //Plays walk animation and lets the enemy circle around using waypoints, once hit they will run away
        protected override void FollowPath() {
            //if (CinematicEnemyHit || IsExecutingAnimation) return;

            IsBusy = true;
            //Change speed randomly
            /*ChangeSpeedTimer += Time.deltaTime;
            if (ChangeSpeedTimer >= ChangeTimeMax) {
                NewSpeed = MaxSpeed * Random.Range(0f, 2f);
                ChangeTimeMax = Random.Range(3f, 11f);
                ChangeSpeedTimer = 0f;
            }*/

            //Make the transitions smoothly
            //NavMeshAgent.speed = Mathf.Lerp(NavMeshAgent.speed, NewSpeed, Time.deltaTime * 1f);

            NavMeshAgent.speed = Speed;
            List<waypoint> waypoints = creatureSpawnController.GroundCinematicEnemyWaypoints;

            if (NavMeshAgent.remainingDistance <= wayPointsRemainedDistance && waypoints.Count > 0) {
                GetRandomWaypointForCinematicEnemy();
                NavMesh.CalculatePath(transform.position, GetRandomWaypointForCinematicEnemy().transform.position, NavMesh.AllAreas, Path);
                NavMeshAgent.SetPath(Path);

                /*int rand = Random.Range(0, Enum.GetValues(typeof(Constants.AnimationsTypes)).Length);

                switch (rand) {
                    case (int) Constants.AnimationsTypes.Feed:
                        StartCoroutine(WaitForStartCreatureToFinishAnimation(TimeToWaitForAnimation, Constants.AnimationsTypes.Feed));
                        return;
                    case (int) Constants.AnimationsTypes.LookAround:
                        StartCoroutine(WaitForStartCreatureToFinishAnimation(TimeToWaitForAnimation, Constants.AnimationsTypes.LookAround));
                        return;
                }*/


                //StartMovingAnimation();

            } else {
                IsBusy = false;
            }
        }

        //After the enemy reached the destination, look at the wall
        /*private void LateUpdate() {
            if (Stop) {
                Vector3 lookAt = new Vector3(EndPoint.LookAtGO.transform.position.x, transform.position.y, EndPoint.LookAtGO.transform.position.z) - transform.position;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookAt), 360f), SmoothRotating * Time.deltaTime);
            }
        }*/

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
            EventsManager.onStartEnemyDeath -= CinematicEnemyHitEvent;
        }

        private waypoint GetRandomWaypointForCinematicEnemy() {
            int random = Random.Range(0, creatureSpawnController.GroundCinematicEnemyWaypoints.Count);
            return creatureSpawnController.GroundCinematicEnemyWaypoints[random];
        }

        private void FollowRandomPath() {
            currentPath = Random.Range(0, PatrolPoints.Count);
        }

        private void WaypointReached() {
            if (wasSpinning) {
                NavMeshAgent.speed /= 2;
                animator.SetBool("Rolled Up Spin Fast", false);
                wasSpinning = false;
            }
            if (wasSpawnningBugs) wasSpawnningBugs = false;

            if (CurrentPatrolIndex + 1 < PatrolPoints[currentPath].Waypoints.Count) {
                ++CurrentPatrolIndex;

                ChangePath();
                SetGroundedEnemyWaypoint();

                int actionType = Random.Range(0, 2); //0 = (shoot or dig), 1 = (spawn bugs or spin)

                if (MakeSpecialAction()) {
                    //Attack using guns if creature is fighter and only shoot by 50% chance and first start shooting when offset was reached
                    if (actionType == 0) {
                        if (Creature is BossCreature) {
                            NavMeshAgent.speed = 0f;
                            StartMovingAnimation();

                            FighterRoutine = EnableShootingForEnemy();
                            StartCoroutine(FighterRoutine);
                        } else if (Creature is IFightingCreature) {
                            //if (CurrentPatrolIndex > Creature.WaypointMinIndexToActivateSpecialActions) {
                            NavMeshAgent.speed = 0f;
                            StartMovingAnimation();

                            FighterRoutine = EnableShootingForEnemy();
                            StartCoroutine(FighterRoutine);
                            //}
                        }
                    } else if (Creature is BugsSpawningCreature && actionType == 1 && !wasSpawnningBugs) {
                        //if (CurrentPatrolIndex > Creature.WaypointMinIndexToActivateSpecialActions) {
                        wasSpawnningBugs = true;
                        GetComponent<BugsSpawningCreature>().OrderToSpawnCreatures();
                        //}
                    } else if (Creature is DiggingCreature) {
                        if (CurrentPatrolIndex + 1 < PatrolPoints[currentPath].Waypoints.Count) {
                            if (!wasDigging && actionType == 0) {
                                NavMeshAgent.speed = 0f;
                                StartMovingAnimation();
                                StartCoroutine(EnableDigging());
                            } else if (!wasSpinning && actionType == 1) {
                                NavMeshAgent.speed *= 2;
                                animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Spin));
                                animator.SetBool("Rolled Up Spin Fast", true);
                                wasSpinning = true;
                            }
                        }
                    } else if (Creature is TransportingCreature && CurrentPatrolIndex + 1 < PatrolPoints[currentPath].Waypoints.Count && !wasTeleporting) {
                        NavMeshAgent.speed = 0f;

                        StartCoroutine(Teleport());
                    }
                }
            } else {
                LastWaypointReached();
            }
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
            Creature.AttackUsingGun();

            yield return new WaitForSeconds(TotalShootTime);

            //If creature not dead and not at wall, continue moving
            if (!NavMeshAgent.enabled && Creature.CurrentState != Creature.CreatureState.Dead && !EndPoint && !Stop) {
                //Creature.OrderToStopShooting();

                NavMeshAgent.enabled = true;

                SetGroundedEnemyWaypoint();
                NavMeshAgent.speed = Speed;

                StopCoroutine(FighterRoutine);
            } else {
                //Creature.OrderToStopShooting();
                StopCoroutine(FighterRoutine);
            }
        }

        private IEnumerator EnableDigging() {
            wasDigging = true;
            Transform toGo = PatrolPoints[currentPath].Waypoints[CurrentPatrolIndex + 1].transform;
            var digger = GetComponent<DiggingCreature>();

            digger.DigDown(toGo);

            yield return new WaitForSeconds(TotalDiggingTime);

            wasDigging = false;
            ++CurrentPatrolIndex;

            NavMeshAgent.enabled = true;

            SetGroundedEnemyWaypoint();
            NavMeshAgent.speed = Speed;
        }

        private IEnumerator Teleport() {
            wasTeleporting = true;
            NavMeshAgent.enabled = false;
            GetComponent<TransportingCreature>().OrderToTransport(PatrolPoints[currentPath].Waypoints[CurrentPatrolIndex + 1].transform);

            yield return new WaitForSeconds(TotalTeleportTime);

            wasTeleporting = false;
            ++CurrentPatrolIndex;

            NavMeshAgent.enabled = true;

            SetGroundedEnemyWaypoint();
            NavMeshAgent.speed = Speed;
        }

        private void LastWaypointReached() {
            waypoint newPos = GameHandler.GetSpot(Creature.Type);

            EndPoint = newPos;
            SetEndPoint(newPos);
            isDamaging = true;
        }

        //Goes to the attack spot
        private void SetEndPoint(waypoint newPos) {
            if (NavMesh.CalculatePath(transform.position, newPos.gameObject.transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
        }

        //Sets path of ground enemies
        public void SetGroundedEnemyWaypoint() {
            if (!NavMeshAgent || !NavMeshAgent.enabled || Creature.CurrentState == Creature.CreatureState.Dead) return;

            NavMesh.CalculatePath(transform.position, PatrolPoints[currentPath].Waypoints[CurrentPatrolIndex >= PatrolPoints[currentPath].Waypoints.Count ? CurrentPatrolIndex - 1 : CurrentPatrolIndex].transform.position, NavMesh.AllAreas, Path);
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
                NavMeshAgent.speed = Speed;

                Creature.AttackerId = null;
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

                if (NavMesh.CalculatePath(transform.position, creatureSpawnController.RunningAwayPoints[Random.Range(0, creatureSpawnController.RunningAwayPoints.Length)].position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
            } else {
                List<waypoint> waypoints = creatureSpawnController.GroundCinematicEnemyWaypoints;

                NavMeshAgent.speed = Speed;

                if (NavMesh.CalculatePath(transform.position, GetRandomWaypointForCinematicEnemy().transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);

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

                NavMeshAgent.speed = Speed + HypnotizedSpeedBonus;

                if (NavMesh.CalculatePath(transform.position, enemyToTrack.transform.position, NavMesh.AllAreas, Path)) NavMeshAgent.SetPath(Path);
            } else if (!creature) {
                GetHypnotized();
            }
        }

        public void EndRoutine() {
            if (Coroutine == null) return;

            StopCoroutine(Coroutine);
        }

        //Change path with a certain chance
        private void ChangePath() {
            int rand = Random.Range(5, 10);
            if (rand == 0) {
                int rand2 = Random.Range(0, PatrolPoints.Count);

                if (CurrentPatrolIndex + 1 >= PatrolPoints[rand2].Waypoints.Count)
                    return;

                ++CurrentPatrolIndex;
                currentPath = rand2;
            }
        }

        /// <summary>
        ///     Gets hit by hypnotized arrow and find closest enemy
        /// </summary>
        public void GetHypnotized(bool findNewEnemy = false) {
            if (IsHypnotized && !findNewEnemy) return;

            List<GameObject> fixedList = GameHandler.AllEnemies.FindAll(e => {
                var creature = e.GetComponent<Creature>();

                if (!creature || !creature.enabled) return false;

                if (creature.EnemyId == Creature.EnemyId) return false;

                if (creature.Type != Creature.CreatureType.Grounded) return false;

                return true;
            });

            if (fixedList.Count <= 0) {
                Creature.Suicide();
                IsHypnotized = false;

                return;
            }

            fixedList.Sort((e1, e2) => {
                float distance1 = Vector3.Distance(transform.position, e1.gameObject.transform.position);
                float distance2 = Vector3.Distance(transform.position, e2.gameObject.transform.position);

                return distance1.CompareTo(distance2);
            });

            TrackEnemyID = fixedList[0].GetComponent<Creature>().EnemyId;
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

                            Speed /= speedDivider;
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

                            Speed *= speedDivider;
                            creature.GetComponent<FlyingCreature>().SetPatrollingSpeed(speedDivider, false);
                        }
                    }
                }
            }
        }

        private IEnumerator UpdateCreatureSpeed() {
            if (Creature.Type == Creature.CreatureType.Flying) {
                Speed /= 4;
                yield return new WaitForSeconds(.5f);
                Speed *= 4;
            } else {
                Speed /= 4;
                yield return new WaitForSeconds(.5f);
                Speed *= 4;
            }
        }
    }
}