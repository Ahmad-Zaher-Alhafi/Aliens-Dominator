using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;
using System.Collections.Generic;

public class NPCSimplePatrol : MonoBehaviour
{
    [SerializeField]
    public bool _patrolWaiting;

    private bool Stop = false;

     [SerializeField]
    float _totalWaitTime = 0f;
    [SerializeField]
    private float wayPointsRemainedDistance;//the distance between the creature and the waypoint that he is going to which allow him to go to the next point
    [SerializeField]
    float _switchProbability = 0.0f;

    [HideInInspector]
    public List<Paths> PatrolPoints = new List<Paths>();

    [HideInInspector]
    public bool isDamaging = false;

    private GameHandler GameHandler;
    [HideInInspector]
    public NavMeshAgent NavMeshAgent;
    public int CurrentPatrolIndex;
    private bool _travelling;
    private bool _waiting;
    private float _waitTimer;
    private bool wasDigging = false;
    private bool wasSpinning = false;
    private bool wasTeleporting = false;
    private bool wasSpawnningBugs = false;
    [HideInInspector]
    public string EnemyId;
    private Animator animator;
    [HideInInspector]
    public Spawner Spawner;

    public string WalkAnimationParameter;
    //public string AttackAnimationParameter;
    private Creature Creature;

    private bool IsHypnotized = false;

    public float ApplyDamageWhenHypnotized = 5f;
    public int CinematicEnemiesPath = 9;
    private NavMeshPath Path;

    //Only for the cinematic enemies
    [Header("Cinematic enemy")]
    private int StartPoint = 0;
    [SerializeField] private float TimeToWaitForAnimation = 3f;
    [SerializeField] private float TimeToRoar = 1f;

    private bool CinematicEnemyHit = false;

    [Header("Rotating towards wall")]
    public float SmoothRotating = 8f;

    private void OnEnable()
    {
        EnemyId = null;
        wasDigging = false;
        wasSpinning = false;
        wasTeleporting = false;
        wasSpawnningBugs = false;

        isDamaging = false;
        Stop = false;

        CurrentPatrolIndex = 0;
        StartPoint = 0;

        _patrolWaiting = false;
    }

	private void OnDisable()
	{
        StopAllCoroutines();
	}

	public void Awake()
    {
        TrackEnemyID = null;

        Path = new NavMeshPath();

        GameHandler = FindObjectOfType<GameHandler>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        Creature = GetComponent<Creature>();

        Spawner = FindObjectOfType<Spawner>();

        if (gameObject.tag == "OnStartWaves")
        {
            GetRandomWaypointForCinematicEnemy();
        }

        FollowRandomPath();

        //You see it right, I'm using events haha
        //This will fire once one enemy got killed/ hit, so enemies stop running in circles
        EventsManager.onStartEnemyDeath += CinematicEnemyHitEvent;

        if(NavMeshAgent)
        {
            if (PatrolPoints.Count <= 0)
            {
                return;
            }

            if(PatrolPoints[currentPath].Waypoints.Count > 0 && PatrolPoints[currentPath].Waypoints.Count >= 2)
            {
                CurrentPatrolIndex = 0;
            }
            else
            {
                Debug.LogError("Insufficient patrol points for basic patrolling behaviour");
            }
        }
    }

    private void GetRandomWaypointForCinematicEnemy()
    {
        StartPoint = UnityEngine.Random.Range(0, Spawner.GroundCinematicEnemyWaypoints.Count);
    }

    private void FollowRandomPath()
    {
        currentPath = UnityEngine.Random.Range(0, PatrolPoints.Count);
    }

    private void WaypointReached()
    {
        if (wasSpinning)
        {
            NavMeshAgent.speed /= 2;
            animator.SetBool("Rolled Up Spin Fast", false);
            wasSpinning = false;
        }
        if (wasSpawnningBugs)
        {
            wasSpawnningBugs = false;
        }

        if (CurrentPatrolIndex + 1 < PatrolPoints[currentPath].Waypoints.Count)
        {
            ++CurrentPatrolIndex;

            ChangePath();
            SetGroundedEnemyWaypoint();

            int actionType = UnityEngine.Random.Range(0, 2);//0 = (shoot or dig), 1 = (spawn bugs or spin)

            if (MakeSpecialAction())
            {
                //Attack using guns if creature is fighter and only shoot by 50% chance and first start shooting when offset was reached
                if (Creature.IsItFighter && actionType == 0)
                {
                    if (!Creature.IsItBoss)
                    {
                        if (CurrentPatrolIndex > Creature.WaypointOffsetToActivateSpecialActions)
                        {
                            NavMeshAgent.speed = 0f;
                            StartMovingAnimation();

                            FighterRoutine = EnableShootingForEnemy();
                            StartCoroutine(FighterRoutine);
                        }
                    }
                    else
                    {
                        NavMeshAgent.speed = 0f;
                        StartMovingAnimation();

                        FighterRoutine = EnableShootingForEnemy();
                        StartCoroutine(FighterRoutine);
                    }
                }
                else if (!Creature.IsItBoss && Creature.IsItBugsSpawner && actionType == 1 && !wasSpawnningBugs)
                {
                    if (CurrentPatrolIndex > Creature.WaypointOffsetToActivateSpecialActions)
                    {
                        wasSpawnningBugs = true;
                        GetComponent<BugsSpawner>().OrderToSpawnCreatures(false);
                    }
                }
                else if (Creature.IsItDigger)
                {
                    if (CurrentPatrolIndex + 1 < PatrolPoints[currentPath].Waypoints.Count)
                    {
                        if (!wasDigging && actionType == 0)
                        {
                            NavMeshAgent.speed = 0f;
                            StartMovingAnimation();
                            StartCoroutine(EnableDigging());
                        }
                        else if (!wasSpinning && actionType == 1)
                        {
                            NavMeshAgent.speed *= 2;
                            animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Spin));
                            animator.SetBool("Rolled Up Spin Fast", true);
                            wasSpinning = true;
                        }
                    }
                }
                else if (Creature.IsItTransporter && CurrentPatrolIndex + 1 < PatrolPoints[currentPath].Waypoints.Count && !wasTeleporting)
                {
                    NavMeshAgent.speed = 0f;

                    StartCoroutine(Teleport());
                }
            }
        }
        else
        {
            LastWaypointReached();
        }
    }

    public void SetCreatureSpeed(float speed)
    {
        NavMeshAgent.speed = speed;
    }

    private bool MakeSpecialAction()
    {
        int chance = Creature.ChanceToMakeSpecialAction;
        int selectedNum = UnityEngine.Random.Range(1, 101);

        if (selectedNum <= chance)
        {
            return true;
        }

        return false;
    }

    private IEnumerator FighterRoutine = null;

    private IEnumerator EnableShootingForEnemy()
    {
        if (EndPoint || Stop || Creature.IsDead)
        {
            StopCoroutine(FighterRoutine);
            yield break;
        }

        //Stop moving and init shooting
        NavMeshAgent.enabled = false;
        Creature.AttackUsingGun();

        yield return new WaitForSeconds(Creature.TotalShootTime);

        //If creature not dead and not at wall, continue moving
        if (!NavMeshAgent.enabled && !Creature.IsDead && !EndPoint && !Stop)
        {
            Creature.OrderToStopShooting();

            NavMeshAgent.enabled = true;

            SetGroundedEnemyWaypoint();
            NavMeshAgent.speed = Creature.EnemySpeed;

            StopCoroutine(FighterRoutine);
        }
        else
        {
            Creature.OrderToStopShooting();
            StopCoroutine(FighterRoutine);
        }
    }

    private IEnumerator EnableDigging()
    {
        wasDigging = true;
        Transform toGo = PatrolPoints[currentPath].Waypoints[CurrentPatrolIndex + 1].transform;
        Digger digger = GetComponent<Digger>();

        digger.DigDown(toGo);

        yield return new WaitForSeconds(Creature.TotalDiggingTime);

        wasDigging = false;
        ++CurrentPatrolIndex;

        NavMeshAgent.enabled = true;

        SetGroundedEnemyWaypoint();
        NavMeshAgent.speed = Creature.EnemySpeed;
    }

    private IEnumerator Teleport()
    {
        wasTeleporting = true;
        NavMeshAgent.enabled = false;
        GetComponent<Transporter>().OrderToTransport(PatrolPoints[currentPath].Waypoints[CurrentPatrolIndex + 1].transform);

        yield return new WaitForSeconds(Creature.TotalTeleportTime);

        wasTeleporting = false;
        ++CurrentPatrolIndex;

        NavMeshAgent.enabled = true;

        SetGroundedEnemyWaypoint();
        NavMeshAgent.speed = Creature.EnemySpeed;
    }

    private void LastWaypointReached()
    {
        var newPos = GameHandler.GetSpot(Creature.EnemyType);

        EndPoint = newPos;
        SetEndPoint(newPos);
        isDamaging = true;
    }

    //Goes to the attack spot
    private void SetEndPoint(waypoint newPos)
    {
        if (NavMesh.CalculatePath(transform.position, newPos.gameObject.transform.position, NavMesh.AllAreas, Path))
        {
            NavMeshAgent.SetPath(Path);
        }
    }

    //Sets path of ground enemies
    public void SetGroundedEnemyWaypoint()
    {
        if (!NavMeshAgent || !NavMeshAgent.enabled || Creature.IsDead)
        {
            return;
        }

        NavMesh.CalculatePath(transform.position, PatrolPoints[currentPath].Waypoints[CurrentPatrolIndex >= PatrolPoints[currentPath].Waypoints.Count ? CurrentPatrolIndex - 1 : CurrentPatrolIndex].transform.position, NavMesh.AllAreas, Path);
        NavMeshAgent.SetPath(Path);
    }

    //Sets destroy path of cinematic enemies once one of them was hit
    private void CinematicEnemyHitEvent()
    {
        if (CompareTag("OnStartWaves"))
        {
            StartCoroutine(WaitForStartCreatureToFinishAnimation(TimeToRoar, Constants.AnimationsTypes.Roar, true));
        }
    }

    private float DistanceFromWaypoint = 0f;
    private float UpdatePathTimer = 0f;
       
    public void Update()
    {
        if (!CompareTag("OnStartWaves"))
        {
            StartMovingAnimation();

            //Dont move anymore when attacking wall
            if (Stop || Creature.WasDied)
            {
                return;
            }

            if (!NavMeshAgent.enabled)
            {
                return;
            }

            if (Timer <= TimeUntilNextHit)
            {
                Timer += Time.deltaTime;
            }

            DistanceFromWaypoint = NavMeshAgent.remainingDistance;
  
            if (IsHypnotized && DistanceFromWaypoint <= 0.3f)
            {
                FollowEnemyWhileHypnotized();
                return;
            }
            else if(IsHypnotized && DistanceFromWaypoint > 0.3f)
            {
                NavMeshAgent.speed = Creature.EnemySpeed + Creature.HypnotizedSpeedBonus;

                GameObject go = EnemyToFollow;

                if (!go)
                {
                    GetHypnotized();
                    return;
                }

                if (UpdatePathTimer >= 0.5f)
                {
                    if (NavMesh.CalculatePath(transform.position, go.transform.position, NavMesh.AllAreas, Path))
                    {
                        NavMeshAgent.SetPath(Path);
                    }

                    UpdatePathTimer = 0f;
                }

                if (UpdatePathTimer <= 0.5f)
                {
                    UpdatePathTimer += Time.deltaTime;
                }

                return;
            }

            //Reached waypoint
            if (DistanceFromWaypoint <= 0.5f && !isDamaging && !isDefending)
            {
                WaypointReached();
                return;
            }

            //Reached attack point
            if(DistanceFromWaypoint <= 0.2f && isDamaging && !isDefending)
            {
                NavMeshAgent.speed = 0f;
                NavMeshAgent.enabled = false;

                Coroutine = StartCoroutine(AttackWall());

                //Stop movement
                Stop = true;

                return;
            }

            if (isDefending)
            {
                Defense();
            }
        }
        else if(CompareTag("OnStartWaves") && !CinematicEnemyHit)
        {
            UpdateCinematicEnemyPosition();
        }
        else if(CompareTag("OnStartWaves") && CinematicEnemyHit)
        {
            if (NavMeshAgent.remainingDistance <= 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    //After the enemy reached the destination, look at the wall
    private void LateUpdate()
    {
        if (Stop)
        {
            Vector3 lookAt = new Vector3(EndPoint.LookAtGO.transform.position.x, transform.position.y, EndPoint.LookAtGO.transform.position.z) - transform.position;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookAt), 360f), SmoothRotating * Time.deltaTime);
        }
    }

    //Attack enemy if not null and in a certain time interval
    private void FollowEnemyWhileHypnotized()
    {
        GameObject enemy = EnemyToFollow;

        if (!enemy)
        {
            GetHypnotized(true);
            return;
        }

        if (Timer >= TimeUntilNextHit)
        {
            animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Attack));

            NavMeshAgent.speed = 0.3f;

            enemy.GetComponent<Creature>().ReceiveDamageFromEnemy(gameObject, Creature.AttackDamage, 100f);

            Timer = 0f;

            return;
        }
    }

    private void Defense()
    {
        GameObject enemy = EnemyToFollow;

        if (!enemy)
        {
            NavMeshAgent.speed = Creature.EnemySpeed;

            Creature.AttackerId = null;
            TrackEnemyID = null;
            isDefending = false;

            SetGroundedEnemyWaypoint();
            return;
        }

        if (Timer >= TimeUntilNextHit)
        {
            animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Attack));
            enemy.GetComponent<Creature>().ReceiveDamageFromEnemy(gameObject, Creature.AttackDamage, 100f);

            Timer = 0f;
        }
    }

    private float ChangeTimeMax = 0f;
    private float ChangeSpeedTimer = 0f;
    private float NewSpeed;

    /// <summary>
    /// Only applies for start enemies, so they stop moving or creating paths while playing an animation
    /// </summary>
    private bool IsExecutingAnimation = false;

    //Plays walk animation and lets the enemy circle around using waypoints, once hit they will run away
    private void UpdateCinematicEnemyPosition()
    {
        if(CinematicEnemyHit || IsExecutingAnimation)
        {
            return;
        }

        //Change speed randomly
        ChangeSpeedTimer += Time.deltaTime;
        if (ChangeSpeedTimer >= ChangeTimeMax)
        {
            NewSpeed = Creature.MaxSpeed * UnityEngine.Random.Range(0f, 2f);
            ChangeTimeMax = UnityEngine.Random.Range(3f, 11f);
            ChangeSpeedTimer = 0f;
        }

        //Make the transitions smoothly
        NavMeshAgent.speed = Mathf.Lerp(NavMeshAgent.speed, NewSpeed, Time.deltaTime * 1f);

        List<waypoint> waypoints = Spawner.GroundCinematicEnemyWaypoints;

        if (NavMeshAgent.remainingDistance <= wayPointsRemainedDistance)
        {
            if (waypoints.Count <= 0)
            {
                return;
            }

            int rand = UnityEngine.Random.Range(0, Enum.GetValues(typeof(Constants.AnimationsTypes)).Length);

            switch(rand)
            {
                case (int)Constants.AnimationsTypes.Feed:
                    StartCoroutine(WaitForStartCreatureToFinishAnimation(TimeToWaitForAnimation, Constants.AnimationsTypes.Feed));
                    return;
                case (int)Constants.AnimationsTypes.LookAround:
                    StartCoroutine(WaitForStartCreatureToFinishAnimation(TimeToWaitForAnimation, Constants.AnimationsTypes.LookAround));
                    return;
            };

            GetRandomWaypointForCinematicEnemy();

            StartMovingAnimation();

            NavMesh.CalculatePath(transform.position, waypoints[StartPoint].transform.position, NavMesh.AllAreas, Path);
            NavMeshAgent.SetPath(Path);
        }
    }

    private IEnumerator WaitForStartCreatureToFinishAnimation(float time, Constants.AnimationsTypes anim, bool cinematicEnemyHit = false)
    {
        IsExecutingAnimation = true;

        NavMeshAgent.speed = 0f;

        StartMovingAnimation();

        animator.Play(Constants.GetAnimationName(gameObject.name, anim));

        yield return new WaitForSeconds(time);

        if (cinematicEnemyHit)
        {
            CinematicEnemyHit = true;
            NavMeshAgent.speed = Creature.MaxSpeed * 2;

            //Update run animation
            StartMovingAnimation();

            if (NavMesh.CalculatePath(transform.position, Spawner.RunningAwayPoints[UnityEngine.Random.Range(0, Spawner.RunningAwayPoints.Length)].position, NavMesh.AllAreas, Path))
            {
                NavMeshAgent.SetPath(Path);
            }
        }
        else
        {
            List<waypoint> waypoints = Spawner.GroundCinematicEnemyWaypoints;

            NavMeshAgent.speed = Creature.EnemySpeed;

            if(NavMesh.CalculatePath(transform.position, waypoints[StartPoint].transform.position, NavMesh.AllAreas, Path))
            {
                NavMeshAgent.SetPath(Path);
            }

            StartMovingAnimation();

            IsExecutingAnimation = false;
        }
    }

    /// <summary>
    /// to activate the animator and start moving animation
    /// </summary>
    public void StartMovingAnimation()
    {
        animator.SetFloat("WalkSpeed", NavMeshAgent.speed / Creature.MaxSpeed);
    }

    private float TimeUntilNextHit = 1f;
    private float Timer = 0f;
    private bool isDefending = false;

    private GameObject EnemyToFollow
    {
        get
        {
            if (TrackEnemyID == null)
            {
                return null;
            }

            GameObject go = GameHandler.AllEnemies.FindAll(enemy => enemy.GetComponent<Creature>()).Find(enemy => enemy.GetComponent<Creature>().EnemyId == TrackEnemyID);
            if (!go)
            {
                return null;
            }

            return go;
        }
    }

    //Find enemy and start decreasing your health
    private void FollowEnemy()
    {
        GameObject enemyToTrack = EnemyToFollow;
        Creature creature = enemyToTrack.GetComponent<Creature>();

        if(enemyToTrack && creature.enabled)
        {
            //Must be set to false, so this enemy walks away from the wall if it is currently attacking the wall
            if (Stop)
            {
                Stop = false;
                StopCoroutine(Coroutine);
            }

            NavMeshAgent.enabled = true;

            StartCoroutine(DecreaseHealth());

            NavMeshAgent.speed = Creature.EnemySpeed + Creature.HypnotizedSpeedBonus;

            if (NavMesh.CalculatePath(transform.position, enemyToTrack.transform.position, NavMesh.AllAreas, Path))
            {
                NavMeshAgent.SetPath(Path);
            }
        }
        else if(!creature)
        {
            GetHypnotized();
        }
    }

    Coroutine Coroutine;

    public void EndRoutine()
    {
        if(Coroutine == null)
        {
            return;
        }
            
        StopCoroutine(Coroutine);
    }

    //Apply damage to wall mainly for grounded enemies
    IEnumerator AttackWall()
    {
        yield return new WaitForSeconds(3f);

        if (isDefending)
        {
            yield break;
        }

        WallManager wallManager = FindObjectOfType<WallManager>();

        animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Attack));
        wallManager.ReduceHealth(Creature.AttackDamage);
        Coroutine = StartCoroutine(AttackWall());
    }

    /// <summary>
    /// This is for hypnotized enemies, so they can't kill all enemies (basically balancing)
    /// </summary>
    /// <returns></returns>
    IEnumerator DecreaseHealth()
    {
        yield return new WaitForSeconds(5f);

        if(!Creature.IsDead)
        {
            Creature.Health -= ApplyDamageWhenHypnotized;

            if(Creature.Health <= 0f)
            {
                Creature.Suicide();
                Debug.Log("Enemy died due to hypnotization");
            }
            else
            {
                StartCoroutine(DecreaseHealth());
            }
        }
    }

    public waypoint EndPoint = null;

    private int currentPath = 0;

    //Change path with a certain chance
    private void ChangePath()
    {
        int rand = UnityEngine.Random.Range(5, 10);
        if(rand == 0)
        {
            int rand2 = UnityEngine.Random.Range(0, this.PatrolPoints.Count);

            if(CurrentPatrolIndex + 1 >= this.PatrolPoints[rand2].Waypoints.Count)
                return;

            ++CurrentPatrolIndex;
            currentPath = rand2;
        }
    }

    [HideInInspector]
    public string TrackEnemyID = null;

    /// <summary>
    /// Gets hit by hypnotized arrow and find closest enemy
    /// </summary>
    public void GetHypnotized(bool findNewEnemy = false)
    {
        if (IsHypnotized && !findNewEnemy)
        {
            return;
        }

        var fixedList = GameHandler.AllEnemies.FindAll(e => {
            Creature creature = e.GetComponent<Creature>();

            if (!creature || !creature.enabled)
            {
                return false;
            }

            if (creature.EnemyId == Creature.EnemyId)
            {
                return false;
            }

            if (creature.EnemyType != EnemyType.Grounded)
            {
                return false;
            }

            return true;
        });

        if (fixedList.Count <= 0)
        {
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
    public void FocusOnAttacker(string enemyId)
    {
        Stop = false;
        EndRoutine();

        TrackEnemyID = enemyId;
        isDefending = true;
        isDamaging = false;

        NavMeshAgent.speed = 0.3f;

        if (NavMesh.CalculatePath(transform.position, EnemyToFollow.transform.position, NavMesh.AllAreas, Path))
        {
            NavMeshAgent.SetPath(Path);
        }
    }

    public void OnDestroy()
    {
        EventsManager.onStartEnemyDeath -= CinematicEnemyHitEvent;
    }
}
