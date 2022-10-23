using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

[System.Serializable]
public class AttackPoint
{
    public waypoint Waypoint;
    public EnemyType EnemyType = EnemyType.Grounded;
}

public class GameHandler : MonoBehaviour
{
    public Transform SpawnPos = null;
    public List<AttackPoint> AttackPoints = new List<AttackPoint>();
    public SecurityWeapon[] SecurityWeapons;
    public List<GameObject> SpecialArrows = new List<GameObject>();
    public List<Ballon> Ballons = new List<Ballon>();
    public float NumOfResources;

    private bool GameEnded = false;
    public Spawner Spawner = null;
    public UIManager UIManager;
    [HideInInspector]
    public List<GameObject> AllEnemies = new List<GameObject>();
    public List<NPCSimplePatrol> CinematicEnemies = new List<NPCSimplePatrol>();

    [HideInInspector]
    public bool WasCinematicCreatuerDied;//true when one of the cinematics creature dies
    [SerializeField] private ParticleSystem arrowUpdateParticlesPhase1;//phase one is particles without noise
    [SerializeField] private ParticleSystem arrowUpdateParticlesPhase2;//ohase two is particles with noise
    [SerializeField] private GameObject oldArrow;//the old arrow that we were using it before the update(needed to put it on the screen and show particles above  it)
    [SerializeField] private GameObject newArrow;//the new arrow after updating(to show it on screen)
    [SerializeField] private int numOfSuppliesToUpdate = 3;//num of spplies that are needed to update your arrow
    [SerializeField] private ArcheryRig archeryRig;
    [SerializeField] private List<GameObject> airplanes = new List<GameObject>();
    [SerializeField] private List<BlockVisionParticles> blockVisionParticles = new List<BlockVisionParticles>();//list of the particles of player block vision
    private int numOfSppliesGathered = 0;//number of supplies object that you have gatherd it till now

    /// <summary>
    /// Handles pooling objects
    /// </summary>
    public PoolManager PoolManager;

    private void Start()
    {
        UpdateResourcesCount(0);
        WasCinematicCreatuerDied = false;
        EventsManager.onCallingSupplies += CallSuppliesAirplane;
        EventsManager.onGatheringSupplies += IncreaseSupplieseCount;
        EventsManager.onStinkyBallHit += BlockPlayerVision;
        NavMesh.pathfindingIterationsPerFrame = 500;

        InitNewWave();
    }

    private void Update()
    {
        if(GameEnded && Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ClearScene()
    {
        var enemies = FindObjectsOfType<Creature>().ToList();

        enemies.ForEach(e => {
            Destroy(e.gameObject);
        });
    }

    //Inits starting a new level
    public void InitNewWave()
    {
        Spawner.Spawn = false;
    }

    public void GameOver(int score)
    {
        UIManager.SetupGameover(score);
        GameEnded = true;
    }

    public void WonGame(int score)
    {
        UIManager.SetupWin(score);
        GameEnded = true;
    }

    public void UpdateStatus(int wave, int maxWave, int level)
    {
        UIManager.UpdateStatus(wave, maxWave, level); 
    }

    public waypoint GetSpot(EnemyType type)
    {
        List<AttackPoint> points = AttackPoints.FindAll(a => a.EnemyType == type);

        return points[Random.Range(0, points.Count)].Waypoint;
    }

    /// <summary>
    /// this function is gonna increase the supplies counter each time you takes a supplies object and when you get enogh of them to update your arrow the UpdateArrow function will be called
    /// </summary>
    public void IncreaseSupplieseCount()
    {
        numOfSppliesGathered++;

        if (numOfSppliesGathered >= numOfSuppliesToUpdate)
        {
            numOfSppliesGathered = numOfSppliesGathered - numOfSuppliesToUpdate;
            StartCoroutine(UpdateArrow());
        }
    }

    /// <summary>
    /// function to update the arrow that you are using it
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateArrow()
    {
        archeryRig.UpdateArrow();
        
        newArrow.SetActive(true);//show the new arrow
        oldArrow.SetActive(true);//show the old arrow
        yield return new WaitForSeconds(.3f);
        arrowUpdateParticlesPhase1.Play();//play phase 1 of updating particels
        yield return new WaitForSeconds(.3f);
        oldArrow.SetActive(false);//hide the old arrow
        yield return new WaitForSeconds(.5f);
        arrowUpdateParticlesPhase2.Play();//play phase 2 of updating particels
        yield return new WaitForSeconds(1.3f);
        newArrow.SetActive(false);//hide the new arrow
    }

    /// <summary>
    /// to call a supplies airplane when you take a supplies caller ballon
    /// </summary>
    public void CallSuppliesAirplane(Constants.SuppliesTypes suppliesType)
    {
        foreach (GameObject airplane in airplanes)
        {
            airplane.SetActive(true);
            Airplane plane = airplane.GetComponent<Airplane>();

            if (!plane.IsItBusy)//if this airplane is not busy
            {
                plane.MoveToDropArea(suppliesType);
                break;
            }
        }
    }

    public void BlockPlayerVision(Constants.ObjectsColors stinkyBallColor)//called when a stinky ball hits the player so his vision should be blocked for a while
    {
        foreach (BlockVisionParticles bVP in blockVisionParticles)//find the right block vision particles color according to the stinky ball color
        {
            if (bVP.BlockVisionParticlesColor == stinkyBallColor)
            {
                ParticleSystem ps = bVP.GetComponent<ParticleSystem>();
                ps.Play();
                break;
            }
        }
    }

    public void UpdateResourcesCount(float resources)
    {
        NumOfResources += resources;
        UIManager.UpdateResourcesNumText(NumOfResources);
    }

    void OnDestroy()
    {
        EventsManager.onGatheringSupplies -= IncreaseSupplieseCount;
        EventsManager.onCallingSupplies -= CallSuppliesAirplane;
        EventsManager.onStinkyBallHit -= BlockPlayerVision;
    }
}
