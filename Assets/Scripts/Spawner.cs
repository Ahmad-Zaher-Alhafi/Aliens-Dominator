using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[System.Serializable]
public class Paths
{
	public List<waypoint> Waypoints;
    public EnemyType EnemyType = EnemyType.Grounded;
}

[System.Serializable]
public class SpawnListItem
{
	public Transform Spawn;
	public List<Paths> Paths;
}

[System.Serializable]
public struct EnemyProperties
{
    public float Health;
    public float Damage;
    public Material Skin;
}

[System.Serializable]
public class EnemyClass
{
    public Creature Enemy;
    public List<EnemyProperties> EnemyProperties = new List<EnemyProperties>(); 
}

[System.Serializable]
public class LevelSettings
{
	public List<EnemyClass> EasyEnemies = new List<EnemyClass>();
	public List<EnemyClass> MediumEnemies = new List<EnemyClass>();
	public List<EnemyClass> HardEnemies = new List<EnemyClass>();
	public List<EnemyClass> BossEnemies = new List<EnemyClass>();

    public int WhenToIncreaseDifficultyOfEnemy = 3;

	[Range(1, 100)]
	public int ChanceOfHarderEnemies = 1;
	public int MaxLevels = 10;
	public int AddEnemiesEachWave = 1;
	public int AddWaveEachLevel = 8;
	[HideInInspector]
	public int EnemyCount = 0;
	[HideInInspector]
	public int CurrentLevel = 0;
	[HideInInspector]
	public int CurrentWave = 0;
	[HideInInspector]
	public int MaxWaves;
	public bool InfiniteLevels = false;

    public float TimerForEnemySpawn = 0f;
    public bool doesItIncludeBoss;

	public void Reset()
	{
		MaxWaves = 0 + AddWaveEachLevel;
		EnemyCount = 0 + AddEnemiesEachWave;
		CurrentLevel = 1;
		CurrentWave = 1;
	}

	public void IncreaseLevel()
	{
		MaxWaves += AddWaveEachLevel;
		EnemyCount += AddEnemiesEachWave;
		CurrentWave = 1;
		++CurrentLevel;
		++ChanceOfHarderEnemies;
	}

	public void IncreaseWave()
	{
		EnemyCount += AddEnemiesEachWave;
		++CurrentWave;
	}

	public GameObject GetEnemy(PoolManager poolManager, int wave, bool isItBoss = false)
	{
        if (isItBoss)
        {
            Debug.Log("Called boss");
            return CreateEnemy(BossEnemies, wave, poolManager);
        }

		int harderEnemies = Random.Range(1, 101);
		if(harderEnemies > ChanceOfHarderEnemies)
		{
			int rand = Random.Range(1, 4);
			if(rand >= 2)
			{
                return CreateEnemy(EasyEnemies, wave, poolManager);

            }
			else
			{
                return CreateEnemy(MediumEnemies, wave, poolManager);
			}
		}
		else
		{
			//int rand = Random.Range(1, 5);
			//if(rand >= 2)
			//{
                return CreateEnemy(HardEnemies, wave, poolManager);
			//}
			//else
			//{
   //             return CreateEnemy(BossEnemies, wave);
			//}
		}
	}

    private GameObject CreateEnemy(List<EnemyClass> enemies, int wave, PoolManager poolManager)
    {
        EnemyClass eClass = enemies[Random.Range(0, enemies.Count)];
        Creature creature = eClass.Enemy;
        List<EnemyProperties> properties = eClass.EnemyProperties;

        if(wave % WhenToIncreaseDifficultyOfEnemy != 0)
        {
            return poolManager.InstantiateCreature(creature.gameObject);
        }

        //due to index, subtract by 1
        int rest = (wave / WhenToIncreaseDifficultyOfEnemy) - 2;

        //Debug.Log(rest);

        if(rest < 0)
        {
            return poolManager.InstantiateCreature(creature.gameObject);
        }

        if(rest >= properties.Count)
        {
            if(rest - 1 < 0 || properties.Count <= 0)
            {
                return poolManager.InstantiateCreature(creature.gameObject);
            }

            rest = properties.Count - 1;

            creature.AttackDamage = properties[rest].Damage;
            creature.Health = properties[rest].Health;

            creature.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = properties[rest].Skin;

            return poolManager.InstantiateCreature(creature.gameObject);
        }

        GameObject go = poolManager.InstantiateCreature(creature.gameObject);

        Creature _creature = go.GetComponent<Creature>();

        _creature.AttackDamage = properties[rest].Damage;
        _creature.Health = properties[rest].Health;

        go.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = properties[rest].Skin;

        return go;
    }
} 

public class Spawner : MonoBehaviour
{
    public Color gizmoColor = Color.red;
	public enum SpawnTypes
    {
		TimedWave
    }

    //This sets the chance of spawning ballons on each creature
	[Range(1, 100)]
	public int ChanceOfSpawningItem = 100;

	public LevelSettings LevelSettings = new LevelSettings();
	public List<SpawnListItem> SpawnList = new List<SpawnListItem>();

	[HideInInspector]
	public int numEnemy = 0;
	[HideInInspector]
	public int killedEnemies = 0;
	[HideInInspector]
	public int spawnedEnemy = 0;
	private int SpawnID;
	[HideInInspector]
	public bool Spawn = false;
	public SpawnTypes spawnType = SpawnTypes.TimedWave;

	[HideInInspector]
	public List<string> Ids = new List<string>();
	private int TotalSpawnedThisWave = 0;

	[HideInInspector]
	public GameHandler GameHandler;

	[HideInInspector]
	public int Score = 0;
    [SerializeField] private Transform[] runningAwayPoints;//points where creatures are gonna run to when the escape from you(cinematic creatures)
    [SerializeField] private GameObject airCreature;
    [SerializeField] private int numOfGroupCreatures;
	//[SerializeField] private Vector3 groupCircleCenterPoint;//the center of the circle shape of the group
    [SerializeField] private Vector2 groupRectangleRowsColumnsNum;//the number of the rows and the columns of the rectangle group

    [SerializeField] private float distanceBetweenEachCreature;//distance between creatuers when forming a shape
    [SerializeField] GameObject creaturePointInGroup;//the point where the air creatre group member is gonna be created
    //[SerializeField] private Creature bossPrefab;
    //[SerializeField] private Transform bossSpwanPoint;
    private bool wasBossSpawned;

    public Transform[] RunningAwayPoints
	{
		get
		{
			return runningAwayPoints;
		}
	}

    public List<waypoint> GroundCinematicEnemyWaypoints = new List<waypoint>();

    public List<waypoint> AirCinematicEnemyWaypoints = new List<waypoint>();

    private float Timer = 0f;

    private void Awake() 
	{
        wasBossSpawned = false;
        Timer = LevelSettings.TimerForEnemySpawn;

        EventsManager.onStartEnemyDeath += StartCinematicView;
        GameHandler = FindObjectOfType<GameHandler>();

		LevelSettings.Reset();
		GameHandler.UpdateStatus(LevelSettings.CurrentWave, LevelSettings.MaxWaves, LevelSettings.CurrentLevel);

        EventsManager.onBossDie += FinishLevel;
        //SpawnAirCreaturesGroup(Constants.GroupsTypes.triangle);
	}
	
	void OnDrawGizmos()
	{
		Gizmos.color = gizmoColor;

        Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
	}

	void Update()
	{
		if(Spawn)
		{
			if(spawnType == SpawnTypes.TimedWave)
			{
				if(!(LevelSettings.InfiniteLevels) && killedEnemies >= LevelSettings.EnemyCount && LevelSettings.CurrentWave >= LevelSettings.MaxWaves && LevelSettings.CurrentLevel >= LevelSettings.MaxLevels)
				{
					GameHandler.WonGame(Score);
					Spawn = false;

					return;
				}
				else if(killedEnemies >= LevelSettings.EnemyCount && LevelSettings.CurrentWave >= LevelSettings.MaxWaves)
                {
                    if (LevelSettings.doesItIncludeBoss && !wasBossSpawned)
                    {
                        SpwanBoss(LevelSettings.doesItIncludeBoss);
                        wasBossSpawned = true;
                        print("Boss spawned");
                    }
                    
					return;
				}
				else if(killedEnemies >= LevelSettings.EnemyCount)
				{
					LevelSettings.IncreaseWave();
					GameHandler.UpdateStatus(LevelSettings.CurrentWave, LevelSettings.MaxWaves, LevelSettings.CurrentLevel);

					TotalSpawnedThisWave = 0;
					killedEnemies = 0;
				}
				else
				{
					SpawnEnemy(false);
				}
			}
		}
	}

    private void SpwanBoss(bool isItBoss)
    {
        SpawnEnemy(isItBoss);
    }

    private void FinishLevel()
    {
        Debug.Log("Level has finished");
        Spawn = false;

        EventsManager.OnLevelFinishs();
        LevelSettings.IncreaseLevel();
        GameHandler.UpdateStatus(LevelSettings.CurrentWave, LevelSettings.MaxWaves, LevelSettings.CurrentLevel);

        TotalSpawnedThisWave = 0;
        killedEnemies = 0;
        wasBossSpawned = false;
    }

	private string GenerateId(int length = 4)
	{
		string[] chars = new string[] { "a", "b", "c", "d", "1", "2", "3", "4" };
		string id = "";

		for(int i = 0; i < length; i++)
		{
			id += chars[Random.Range(0, chars.Length)];
		}

		if(Ids.Exists(e => e == id))
			return GenerateId(length);

		return id;
	}

    /// <summary>
    /// To start the cinimatice view and the creatures will start runnig away if you hit one of them
    /// </summary>
    public void StartCinematicView()
    {
        if(Spawn)
        {
            return;
        }

        foreach (NPCSimplePatrol NPC in GameHandler.CinematicEnemies)
        {
            NPC.StartMovingAnimation();
        }

        OnStartWaves();
    }

    public void OnStartWaves()
    {
        Spawn = true;
        GameHandler.UpdateStatus(LevelSettings.CurrentWave, LevelSettings.MaxWaves, LevelSettings.CurrentLevel);
    }

    public void SpawnEnemy(bool isItBoss)
    {
        if (!isItBoss)
        {
            if (Timer < LevelSettings.TimerForEnemySpawn)
            {
                Timer += Time.deltaTime;
                return;
            }
            else
            {
                Timer = 0f;
            }

            if (TotalSpawnedThisWave + 1 > LevelSettings.EnemyCount)
            {
                return;
            }

            if (spawnedEnemy + 1 > LevelSettings.EnemyCount)
            {
                return;
            }
        }


        // if (spawnedEnemy + 1 > GameHandler.AttackPoints.Count)
        // {
        //     Debug.Log("More spawning enemies than attack points, they have to wait now!");
        // }

        GameObject Enemy = null;
        Transform spawnPos = null;
        EnemyType typeOfEnemy;

        Enemy = LevelSettings.GetEnemy(GameHandler.PoolManager, LevelSettings.CurrentWave, isItBoss);
        Enemy.SetActive(true);

        typeOfEnemy = Enemy.GetComponent<Creature>().EnemyType;
        spawnPos = GetSpawnPos(typeOfEnemy);

        NavMeshAgent _agent = Enemy.GetComponent<NavMeshAgent>();
        if(_agent)
        {
            _agent.enabled = false;
        }

        Enemy.transform.position = spawnPos.position;
        Enemy.transform.rotation = Quaternion.identity;

        if(_agent)
        {
            _agent.enabled = true;
        }

        string id = GenerateId();

        Ids.Add(id);

        List<Paths> waypoints = null;

        waypoints = SpawnList.Find(s => s.Spawn == spawnPos).Paths.FindAll(p => p.EnemyType == typeOfEnemy);

        if (waypoints == null || waypoints.Count <= 0)
        {
            Debug.LogError("No waypoints were added");
            return;
        }

        if (typeOfEnemy == EnemyType.Flying)
        {
            Enemy.GetComponent<FlyingSystem>().CreaturePathes = waypoints;
        }
        else
        {
            NPCSimplePatrol patrol = Enemy.GetComponent<NPCSimplePatrol>();

            if (!patrol)
            {
                return;
            }

            patrol.EnemyId = id;
            patrol.PatrolPoints = waypoints;
            patrol.Spawner = this;
        }

        Creature creature = Enemy.GetComponent<Creature>();
        creature.ChanceOfDroppingArrow = ChanceOfSpawningItem;
        creature.EnemyId = id;

        numEnemy++;
        spawnedEnemy++;
        TotalSpawnedThisWave++;

        GameHandler.AllEnemies.Add(Enemy);
    }

    public GameObject SpawnBug(Transform spawnPoint, GameObject creaturToInstantioate, NPCSimplePatrol BugSpawner)
    {
        GameObject Enemy = null;
        Transform spawnPos = null;
        EnemyType typeOfEnemy;

        Enemy = creaturToInstantioate;
        typeOfEnemy = Enemy.GetComponent<Creature>().EnemyType;
        spawnPos = spawnPoint;


        GameObject enemyInstantiated = Instantiate(Enemy, spawnPos.position, Quaternion.identity);
        Creature creature = enemyInstantiated.GetComponent<Creature>();
        creature.IsItBug = true;
        string id = GenerateId();

        Ids.Add(id);

        List<Paths> waypoints = null;

        waypoints = BugSpawner.PatrolPoints;


        if (waypoints == null || waypoints.Count <= 0)
        {
            Debug.LogError("No waypoints were added");
            return null;
        }

        if (typeOfEnemy == EnemyType.Flying)
        {
            enemyInstantiated.GetComponent<FlyingSystem>().CreaturePathes = waypoints;
        }
        else
        {
            NPCSimplePatrol patrol = enemyInstantiated.GetComponent<NPCSimplePatrol>();
            if (!patrol)
            {
                return null;
            }

            patrol.EnemyId = id;
            patrol.PatrolPoints = waypoints;
            patrol.CurrentPatrolIndex = BugSpawner.CurrentPatrolIndex;
            patrol.Spawner = this;
        }

        creature.ChanceOfDroppingArrow = ChanceOfSpawningItem;
        creature.EnemyId = id;

        numEnemy++;
        spawnedEnemy++;
        TotalSpawnedThisWave++;

        GameHandler.AllEnemies.Add(enemyInstantiated);

        return enemyInstantiated;
    }

    private int LastPos = -1;

    private Transform GetSpawnPos(EnemyType type)
    {
        List<SpawnListItem> items = SpawnList.FindAll(s => s.Paths.Find(p => p.EnemyType == type) != null);

        int rand = Random.Range(0, items.Count);
        if(LastPos == -1)
        {
            LastPos = rand;
            return items[rand].Spawn;
        }
        else 
        {
            if(rand == LastPos)
                return GetSpawnPos(type);
        }

        LastPos = rand;
        return items[rand].Spawn;
    }

    public void SpawnAirCreaturesGroup(Constants.GroupsTypes groupType)
    {
        switch (groupType)
        {
            case Constants.GroupsTypes.circle: SpawnAirCircleGroup(); break;
            case Constants.GroupsTypes.rectangle: SpawnAirRectangleGroup(); break;
            case Constants.GroupsTypes.triangle: SpawnAirTriangleGroup(); break;

            default: break;
        }
    }

    public void SpawnAirCircleGroup()
    {
		GameObject enemyInstantiated;
        FlyingSystem flyingSystem;
        Transform leader = null;

        var spawnPos = GetSpawnPos(EnemyType.Flying);

        for (int i = 0; i < numOfGroupCreatures; i++)
        {
            float angle = i * (360 / (numOfGroupCreatures - 1));
            Vector3 dir = Quaternion.Euler(0, 0, angle) * new Vector3(0, 1);
            Vector3 position = spawnPos.position + dir * distanceBetweenEachCreature;

            if (i == 0)//set the first creature at the center of the circle
            {
                enemyInstantiated = Instantiate(airCreature, spawnPos.position, Quaternion.identity);
                enemyInstantiated.transform.localScale *= 2;
                leader = enemyInstantiated.transform;
                flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                flyingSystem.IsItTheLeader = true;
                flyingSystem.IsItGroupMember = true;
            }
            else
            {
                enemyInstantiated = Instantiate(airCreature, position, Quaternion.identity);
                enemyInstantiated.transform.parent = leader;
                flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                flyingSystem.IsItTheLeader = false;
                flyingSystem.IsItGroupMember = true;
            }

            DoTheRest(enemyInstantiated, leader, flyingSystem, spawnPos);
        }
    }

    public void SpawnAirRectangleGroup()
    {
		//to make sure that the numbers of the rows or the columns are not float
        groupRectangleRowsColumnsNum.x = Mathf.CeilToInt(groupRectangleRowsColumnsNum.x);
        groupRectangleRowsColumnsNum.y = Mathf.CeilToInt(groupRectangleRowsColumnsNum.y);
		//

        GameObject enemyInstantiated;
        FlyingSystem flyingSystem;
        Transform leader = null;

        var spawnPos = GetSpawnPos(EnemyType.Flying);

        Vector3 position = spawnPos.position;
        Vector3 leaderPos = Vector3.zero;

        for (int i = 0; i < groupRectangleRowsColumnsNum.x; i++)
        {
            for (int j = 0; j < groupRectangleRowsColumnsNum.y; j++)
            {
                if (i == 0 && j == 0)//set the first creature at the center of the circle
                {
					if (groupRectangleRowsColumnsNum.x % 2 == 0 && groupRectangleRowsColumnsNum.y % 2 == 0)
					{
                     leaderPos = spawnPos.position + new Vector3((groupRectangleRowsColumnsNum.x / 2 - 1) * distanceBetweenEachCreature, -(groupRectangleRowsColumnsNum.y / 2 - 1) * distanceBetweenEachCreature);

                    }
                    else if(groupRectangleRowsColumnsNum.x % 2 == 0 && groupRectangleRowsColumnsNum.y % 2 != 0)
					{
                     leaderPos = spawnPos.position + new Vector3((groupRectangleRowsColumnsNum.x / 2 - 1) * distanceBetweenEachCreature, -(groupRectangleRowsColumnsNum.y / 2) * distanceBetweenEachCreature);
                    }
                    else if (groupRectangleRowsColumnsNum.x % 2 != 0 && groupRectangleRowsColumnsNum.y % 2 == 0)
                    {
                     leaderPos = spawnPos.position + new Vector3((groupRectangleRowsColumnsNum.x / 2 ) * distanceBetweenEachCreature, -(groupRectangleRowsColumnsNum.y / 2 - 1) * distanceBetweenEachCreature);
                    }
					else
					{
                     leaderPos = spawnPos.position + new Vector3((groupRectangleRowsColumnsNum.x / 2) * distanceBetweenEachCreature, -(groupRectangleRowsColumnsNum.y / 2) * distanceBetweenEachCreature);
                    }

                    enemyInstantiated = Instantiate(airCreature, leaderPos + new Vector3(-.8f,-.5f), Quaternion.identity);
                    enemyInstantiated.transform.localScale *= 2;
                    leader = enemyInstantiated.transform;
                    flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                    flyingSystem.IsItTheLeader = true;
                    flyingSystem.IsItGroupMember = true;
                    position += new Vector3(1, 0, 0) * distanceBetweenEachCreature;
                }
				else if (i == Mathf.CeilToInt(groupRectangleRowsColumnsNum.x / 2) - 1 && j == Mathf.CeilToInt(groupRectangleRowsColumnsNum.y / 2) - 1)
                {
                    enemyInstantiated = Instantiate(airCreature, spawnPos.position, Quaternion.identity);
                    enemyInstantiated.transform.parent = leader;
                    flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                    flyingSystem.IsItTheLeader = false;
                    flyingSystem.IsItGroupMember = true;
                    position += new Vector3(1, 0, 0) * distanceBetweenEachCreature;
                }
                else
                {
                    enemyInstantiated = Instantiate(airCreature, position, Quaternion.identity);
                    enemyInstantiated.transform.parent = leader;
                    flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                    flyingSystem.IsItTheLeader = false;
                    flyingSystem.IsItGroupMember = true;
                    position += new Vector3(1, 0, 0) * distanceBetweenEachCreature;

                }

                DoTheRest(enemyInstantiated, leader, flyingSystem, spawnPos);
            }
            position.x = spawnPos.position.x;
            position -= new Vector3(0, 1, 0) * distanceBetweenEachCreature;
        }
    }

    public void SpawnAirTriangleGroup()
    {
        //to make sure that the numbers of the rows or the columns are not float
        groupRectangleRowsColumnsNum.x = Mathf.CeilToInt(groupRectangleRowsColumnsNum.x);
        groupRectangleRowsColumnsNum.y = Mathf.CeilToInt(groupRectangleRowsColumnsNum.y);
        //

        GameObject enemyInstantiated;
        FlyingSystem flyingSystem;
        Transform leader = null;

        var spawnPos = GetSpawnPos(EnemyType.Flying);

        Vector3 position = spawnPos.position;

        for (int i = 0; i < groupRectangleRowsColumnsNum.x; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                if (i == 0 && j == 0)//set the first creature at the center of the circle
                {
                    enemyInstantiated = Instantiate(airCreature, position + new Vector3(0,-1.2f), Quaternion.identity);
                    enemyInstantiated.transform.localScale *= 2;
                    leader = enemyInstantiated.transform;
                    flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                    flyingSystem.IsItTheLeader = true;
                    flyingSystem.IsItGroupMember = true;
                    position += new Vector3(1, 0, 0) * distanceBetweenEachCreature;
                }
                else
                {
                    enemyInstantiated = Instantiate(airCreature, position, Quaternion.identity);
                    enemyInstantiated.transform.parent = leader;
                    flyingSystem = enemyInstantiated.GetComponent<FlyingSystem>();
                    flyingSystem.IsItTheLeader = false;
                    flyingSystem.IsItGroupMember = true;
                    position += new Vector3(1, 0, 0) * distanceBetweenEachCreature;
                }

                DoTheRest(enemyInstantiated, leader, flyingSystem, spawnPos);
            }
            spawnPos.position = spawnPos.position - new Vector3(+.5f, 0) * distanceBetweenEachCreature;
            position.x = spawnPos.position.x;
            position -= new Vector3(0, 1, 0) * distanceBetweenEachCreature;
        }
    }

    public void DoTheRest(GameObject enemyInstantiated, Transform leader, FlyingSystem flyingSystem, Transform spawnPos)//this function does lot of things and we make it to prevents repeating the code inside it for 3 or 4 times
	{

        GameObject point = Instantiate(creaturePointInGroup, enemyInstantiated.transform.position, Quaternion.identity);//creat a point that where the member position should be in the group
        point.transform.parent = leader;//attach this point to the leader o we make sure that it's gonna move with it
        flyingSystem.CreaturePointInGroup = point.transform;

        string id = GenerateId();

        Ids.Add(id);

        var waypoints = SpawnList.Find(s => s.Spawn == spawnPos).Paths.FindAll(p => p.EnemyType == EnemyType.Flying);

        if (waypoints == null || waypoints.Count <= 0)
        {
            Debug.LogError("No waypoints were added");
            return;
        }


        flyingSystem.CreaturePathes = waypoints;

        Creature creature = enemyInstantiated.GetComponent<Creature>();
        creature.ChanceOfDroppingArrow = ChanceOfSpawningItem;
        creature.EnemyId = id;

        numEnemy++;
        spawnedEnemy++;
        TotalSpawnedThisWave++;

        GameHandler.AllEnemies.Add(enemyInstantiated);
	}

    public void StartNextLevel()
    {
        Spawn = true;
    }

    void OnDestroy()
	{
        EventsManager.onStartEnemyDeath -= StartCinematicView;
        EventsManager.onBossDie -= FinishLevel;
    }
}