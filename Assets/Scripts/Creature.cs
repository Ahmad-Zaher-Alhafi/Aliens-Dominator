using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public enum EnemyType
{
    Grounded,
    Flying
}

[System.Serializable]
public class WeightOnBodyPart
{
    [Range(0f, 5f)]
    public float Weight = 1f;//1 = full damage of the arrow gets applied
    public Constants.BodyParts BodyPart;//The tag where the weight gets applied
}

public class Creature : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody body;
    private Animator animator;

    public Animator Animator
    {
        get
        {
            return animator;
        }
    }

    /// <summary>
    /// Specifies which body part takes how much damage
    /// </summary>
    public List<WeightOnBodyPart> Weights = new List<WeightOnBodyPart>();

    private NPCSimplePatrol movement;
    public EnemyType EnemyType = EnemyType.Grounded;

    [HideInInspector]
    public string EnemyId;
    //public float forceToApplyOnGateDoor;
    public float MaxSpeed = 10f;
    public float HypnotizedSpeedBonus = 1f;

    public float EnemySpeed = 5f;
    public float AttackDamage = 10f;
    public float TotalShootTime = 10f;
    public float TotalDiggingTime = 5f;
    public float TotalTeleportTime = 5f;
    public GameObject CreatureStateCanves;
    public Slider CreatureHealthBar;
    public int ChanceToMakeSpecialAction;
    private float initialHealth;
    public int ScorePoints = 100;
    private NavMeshAgent agent;

    private Spawner Spawner;

    [HideInInspector]
    public bool IsDead = false;

    [HideInInspector]
    public bool WasRevived = false;

    public float hitForce = 1000;

    public float Health = 100f;
    private Rigidbody[] ChildrenRigidbody;

    private GameHandler GameHandler;

    public string WalkAnimationParamter;
    public List<TextMotionBehavior> textsMotionBehavior;
    [HideInInspector]
    public int ChanceOfDroppingArrow = 0;
    [HideInInspector] public bool WasDied;
    public bool IsItBoss;
    public bool IsItFighter;//if this creature has a weapon to shoot using it or not
    public bool IsItBugsSpawner;//if this creature has the ability to spawn creatures from his mouth
    public bool IsItDigger;//if this creature can dig in the ground to go to another waypoint under the ground
    public bool IsItTransporter;//if this creature can transport from a waypoint to another waypoint using the portals
    public bool IsItBug;
    public int WaypointOffsetToActivateSpecialActions = 2;//We dont want the enemies to start shooting from the spawn waypoint
    public float secondsBetweenGunShots;//fire rate
    public float gunBulletSpeed;
    [SerializeField] private ParticleSystem bloodEffect;//blood effect of the creature
    [SerializeField] private float timeToDestroyDeadBody = 3;//time needed before destroying the dead body of the creature
    [SerializeField] private Constants.ObjectsColors creatureColor;
    public GameObject RigBody;//the RigMain object that is inside the creature object
    [SerializeField] private CreatureWeapon[] creatureWeapons;//creature guns
    [SerializeField] private float secondsBetweenRightAndLeftGunsShots;//time between the right bullet from the right gun and the left bullet from the left gun
    private bool isInsideSeacurityArea;//true if it entred the security area of the security weapon
    private Transform playerShootAtPoint;//point where the creature is gonna look at to shoot
    private Transform playerLookAtPoint;//point where the creature canves is gonna look at
    private bool hasToLookAtTheTarget;//if he has to rotates towards the target to start shooting
    private bool isSlowedDown;//true if the enemy is slowed down using a slow down arrow
    private Rigidbody rig;
    public bool IsSlowedDown
    {
        get
        {
            return isSlowedDown;
        }
        set
        {
            isSlowedDown = value;
        }
    }
    public Constants.ObjectsColors CreatureColor
    {
        get
        {
            return creatureColor;
        }
    }

    private void Awake()
    {
        InitEnemy();
    }

    private void OnEnable()
    {
        InitEnemy();
    }

    private void InitEnemy()
    {
        if (IsItBug)
        {
            EventsManager.onBossDie += killBugs;
        }

        CreatureStateCanves.SetActive(true);
        gameObject.SetActive(true);

        rig = GetComponent<Rigidbody>();
        isInsideSeacurityArea = false;
        WasDied = false;
        IsDead = false;
        playerShootAtPoint = GameObject.FindGameObjectWithTag(Constants.PlayerShootAtPoint).transform;
        playerLookAtPoint = GameObject.FindGameObjectWithTag(Constants.PlayerLookAtPoint).transform;

        hasToLookAtTheTarget = false;
        isSlowedDown = false;
        AttackerId = null;

        GameHandler = FindObjectOfType<GameHandler>();

        audioSource = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        movement = GetComponent<NPCSimplePatrol>();

        if (EnemyType == EnemyType.Grounded)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = EnemySpeed;
        }

        if (gameObject.tag != "OnStartWaves")
        {
            animator.SetBool(WalkAnimationParamter, true);
        }

        Spawner = FindObjectOfType<Spawner>();

        ChildrenRigidbody = GetComponentsInChildren<Rigidbody>();

        EnableRagdoll(false, null);
        initialHealth = Health;
        CreatureHealthBar.maxValue = initialHealth;
        CreatureHealthBar.minValue = 0;
        CreatureStateCanves.SetActive(false);
    }

    void Update()
    {
        if (IsItFighter && hasToLookAtTheTarget)//if this creatur has guns and has to start rotating towards the player
        {
            LookAtTheTarget(playerShootAtPoint);
        }

        CreatureStateCanves.transform.LookAt(playerLookAtPoint);
    }

    public void EnableRagdoll(bool enabled, GameObject hit, float force = 150f)
    {
        animator.enabled = !enabled;

        for (int i = 0; i < ChildrenRigidbody.Length; i++)
        {
            if (!ChildrenRigidbody[i].CompareTag("BloodEffect"))
            {
                ChildrenRigidbody[i].isKinematic = !enabled;
                ChildrenRigidbody[i].useGravity = enabled;

                CreatureCollider colCatcher = ChildrenRigidbody[i].GetComponent<CreatureCollider>();
                if (!colCatcher)
                {
                    ChildrenRigidbody[i].gameObject.AddComponent<CreatureCollider>().InitializeCollider(this);
                }
                else
                {
                    colCatcher.InitializeCollider(this);
                }

                if (enabled)
                {
                    ChildrenRigidbody[i].AddExplosionForce(force, ChildrenRigidbody[i].transform.position, 3f);
                }
            }
        }

        body.detectCollisions = !enabled;
        body.isKinematic = !enabled;

        if (agent)
        {
            agent.enabled = !enabled;
        }

        if (EnemyType == EnemyType.Grounded)
        {
            if (movement)
            {
                movement.enabled = !enabled;
            }
        }


        if (hit == null)
        {
            return;
        }

        for (int j = 0; j < ChildrenRigidbody.Length; j++)
        {
            if (!ChildrenRigidbody[j].CompareTag("BloodEffect"))
            {
                ChildrenRigidbody[j].AddForce(hit.transform.forward * hitForce + Vector3.up * hitForce / 4);
            }
        }

        rig.isKinematic = enabled;
        rig.useGravity = !enabled;
    }

    public float AttackTimer = 3f;

    /// <summary>
    /// Handle a normal arrow hit, it considers the body part where the arrow hit too
    /// </summary>
    /// <param name="arrow"></param>
    /// <param name="tag">Tag of body part</param>
    public void HandleHit(ArrowBase arrow, string tag)
    {
        if (IsDead)
        {
            return;
        }

        float multiplier = 1f;

        //Debug.Log(tag);

        //Find the custom weight property set to the body part, if none was found set weight to 1
        WeightOnBodyPart weight = Weights.Find(w => 
        {
            if (!Constants.EnemyBodyParts.ContainsKey(tag))
            {
                return false;
            }

            var pair = Constants.EnemyBodyParts.First(p => p.Value == w.BodyPart);

            if (pair.Key != tag)
            {
                return false;
            }

            return true;
        });

        if (weight != null)
        {
            multiplier = weight.Weight;
        }
        else
        {
            Debug.LogError("No weight setup for this body part, using default value which is 100%");
        }

        float dmg = (arrow.damage * multiplier);

        ShowDamageText(dmg);

        Health -= dmg;

        StartCoroutine(UpdateCreatureSpeed());
        animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.TakeDamage));
        ApplyDamage(arrow.gameObject, hitForce, arrow.transform.position);
    }

    private void ShowDamageText(float dmg)
    {
        if (!CreatureStateCanves.gameObject.activeInHierarchy)
        {
            CreatureStateCanves.SetActive(true);
        }

        bool hasFoundFreeText = false;

        foreach (TextMotionBehavior text in textsMotionBehavior)//find a free text
        {
            if (!text.IsItBusy)
            {
                hasFoundFreeText = true;
                text.ShowDamageTakenText(dmg);
            }
        }

        if (!hasFoundFreeText)//if it has not found a free text then create new one
        {
            TextMotionBehavior newText = Instantiate(textsMotionBehavior[0], textsMotionBehavior[0].transform.parent);
            textsMotionBehavior.Add(newText);
            newText.ShowDamageTakenText(dmg);
        }
    }

    public void ReceiveExplosionDamage(ArrowBase arrow, float dmg, float force)
    {
        if (IsDead)
        {
            return;
        }

        Health -= dmg;

        StartCoroutine(UpdateCreatureSpeed());
        animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.TakeDamage));
        ApplyDamage(arrow.gameObject, force, arrow.transform.position);
    }

    public void ReceiveDamageFromObject(float dmg, float force, Vector3 damagedPosition = new Vector3())
    {
        if (IsDead)
        {
            return;
        }

        Health -= dmg;
        StartCoroutine(UpdateCreatureSpeed());
        animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.TakeDamage));

        ApplyDamage(null, force, damagedPosition);
    }

    [HideInInspector]
    public string AttackerId = null;

    public void ReceiveDamageFromEnemy(GameObject enemy, float dmg, float force)
    {
        if (IsDead)
        {
            return;
        }

        StartCoroutine(UpdateCreatureSpeed());
        animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.TakeDamage));

        Health -= dmg;

        if (AttackerId == null && movement.TrackEnemyID == null)
        {
            //Debug.Log("Fight back");

            if (!agent)
            {
                return;
            }

            agent.enabled = true;
            AttackerId = enemy.GetComponent<Creature>().EnemyId;
            movement.FocusOnAttacker(AttackerId);
        }

        ApplyDamage(enemy, force);
    }

    public void Hypnotize(float bonusHealth)
    {
        if (IsDead)
        {
            return;
        }

        Health += bonusHealth;
        movement.GetHypnotized();
    }

    /// <summary>
    /// Revive the enemy
    /// </summary>
    /// <param name="bonusHealth"></param>
    /// <param name="bodyPoint">the point where the arrow hit the body</param>
    public void BringBackToLife(float bonusHealth, Transform bodyPoint)
    {
        //print("Back To Life");
        //animator.enabled = true;
        if (!IsDead || EnemyType != EnemyType.Grounded)
        {
            return;
        }

        transform.position = bodyPoint.position;
        //transform.rotation = Quaternion.identity;
        EnableRagdoll(false, gameObject);
        //Reset the agent, otherwise it wont detect the NavMesh
        //if (agent && !agent.isOnNavMesh)
        //{
            agent.enabled = false;
            agent.enabled = true;
        //}

        WasRevived = true;
        IsDead = false;
        WasDied = false;
        Spawner.Ids.Add(EnemyId);
        GameHandler.AllEnemies.Add(gameObject);
        Health = 100 + bonusHealth;
        movement.GetHypnotized();
    }

    public void Suicide()
    {
        if (IsDead)
        {
            return;
        }

        Health = 0f;
        ApplyDamage(null, 100f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="force"></param>
    /// <param name="damagedPosition">it's the position where the arrow hit the creature if it was hit by an arrow otherwise it's null and blood will be in the meddle of the creautre</param>
    private void ApplyDamage(GameObject hit, float force, Vector3 damagedPosition = new Vector3())
    {
        if (!CreatureStateCanves.gameObject.activeInHierarchy)
        {
            CreatureStateCanves.SetActive(true);
        }

        CreatureHealthBar.normalizedValue = Health / initialHealth;
        ActivateBloodEffect(damagedPosition);//activate the blood effect

        if (Health <= 0f && !WasDied)
        {   
            PlayDeathSound();//play creature death sound
            WasDied = true;

            if (isInsideSeacurityArea)//if it was inside the security area then call an event to remove it from the targets list of the security sensor
            {
                EventsManager.OnEnemyDiesInsideSecurityArea(this);
            }

            if (EnemyType == EnemyType.Flying)
            {
                GetComponent<FlyingSystem>().StopMovingWhenDie();//to prevent the air creature from moving after being hit
            }

            if (!GameHandler.WasCinematicCreatuerDied && CompareTag("OnStartWaves"))
            {
                GameHandler.WasCinematicCreatuerDied = true;
                GameHandler.CinematicEnemies.Remove(gameObject.GetComponent<NPCSimplePatrol>());//remove the dead creature from the list
                EventsManager.OnStartEnemyDeath();//call an event that says one of the start enemies was died
            }

            int rand = Random.Range(1, 101);
            if (rand <= ChanceOfDroppingArrow)
            {
                SpawnBallon();
            }

            Spawner.Ids.Remove(EnemyId);

            EnableRagdoll(true, hit);

            if (hit)
            {
                body.AddForce(hit.transform.forward * force);
            }

            if (CompareTag("OnStartWaves"))
            {
                timeToDestroyDeadBody += 5;//to give the air creature exestra time to fall down before it gets destroied
            }
            else
            {
                if (this.EnemyType == EnemyType.Flying && GetComponent<FlyingSystem>().IsItTheLeader)//if this creature was a air group leader
                {
                    EventsManager.OnAirLeaderDeath();//call an event to break the group and unparent the children from the leader
                }
            }

            StartCoroutine(DestroyEnemyObject());

            if (IsItBoss)
            {
                EventsManager.OnBossDie();
            }

            if (gameObject.tag != "OnStartWaves")
            {
                //print("Recording");
                RagdollRewind ragdollRewind = RigBody.GetComponent<RagdollRewind>();
                ragdollRewind.StartRecording(1, .1f, animator);

                if (movement)
                    movement.EndRoutine();

                --Spawner.spawnedEnemy;
                ++Spawner.killedEnemies;

                GameHandler.Spawner.Score += ScorePoints;
                GameHandler.AllEnemies.RemoveAt(GameHandler.AllEnemies.FindIndex(e => e == gameObject));
            }

            IsDead = true;
        }
    }

    private IEnumerator DestroyEnemyObject()
    {
        yield return new WaitForSeconds(timeToDestroyDeadBody);

        if (WasRevived)
        {
            WasRevived = false;
            yield break;
        }

        if (!CompareTag("OnStartWaves"))
        {
            CreatureStateCanves.SetActive(false);
            gameObject.SetActive(false);

            List<ArrowBase> _arrows = GetComponentsInChildren<ArrowBase>().ToList();

            //Make sure we remove all arrows from creature before despawning it
            foreach (ArrowBase _arrow in _arrows)
            {
                _arrow.transform.SetParent(null);
                _arrow.gameObject.SetActive(false);

                GameHandler.PoolManager.AddArrowToPool(_arrow.gameObject);
            }

            GameHandler.PoolManager.AddCreature(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnBallon()
    {
        Instantiate(GetBallon().gameObject, transform.position + Vector3.up, Quaternion.identity);
    }

    /*We need to generate a list of the correct amount of items (like if one item = 75%, there will be 75 items in the list (since I set max to 100)). 
    * Then it picks a random number and returns the object
    * Side note: caching this could cause some items to never get selected.
    * Note: Make sure all chances together = 100% otherwise it could return null
    */
    private Ballon GetBallon(int max = 100)
    {
        List<Ballon> list = new List<Ballon>();

        GameHandler.Ballons.ForEach(b =>
        {
            float chance = Mathf.RoundToInt((max / 100) * b.ChanceOfSpawning);

            for (var i = 0; i < chance; i++)
            {
                list.Add(b);
            }
        });

        return list[Random.Range(0, list.Count)];
    }

    //Zaher added this

    /// <summary>
    /// function to play blood effect particle system when the creature get hit by an arrow
    /// </summary>
    /// <param name="damagedPosition">it's the position where the arrow hit the creature if it was hit by an arrow otherwise it's null and blood will be in the meddle of the creautre</param>
    public void ActivateBloodEffect(Vector3 damagedPosition)
    {
        if (bloodEffect != null)
        {
            if (damagedPosition == null)
            {
                damagedPosition = transform.position;
            }

            bloodEffect.transform.position = damagedPosition;//let the particles be in the same posiotin where the arrow hit the creature
            bloodEffect.gameObject.SetActive(true);
            bloodEffect.Play();
        }
    }

    /// <summary>
    /// function to play creature death sound when it dies
    /// </summary>
    public void PlayDeathSound()
    {
        if (CompareTag("OnStartWaves"))//play scream sound if it was the first enemy so after that the army of creatures start attacking you
        {
            AudioManager.Instance.PlayScreamSound();
        }

        audioSource.Play();
    }

    private void killBugs()
    {
        Suicide();
    }

    private void LookAtTheTarget(Transform target)//to make the player look toward the target (the player for now)
    {
        Quaternion oldQuat = transform.rotation;
        transform.LookAt(target);
        float targetYAngle = transform.rotation.y;
        transform.rotation = oldQuat;
        Quaternion targetAngle = new Quaternion(transform.rotation.x, targetYAngle, transform.rotation.z, transform.rotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, .05f);

        if (Quaternion.Angle(transform.rotation, targetAngle) <= .2f)//if the creature finished rotating to the correct angle wihch is the player angle
        {
            hasToLookAtTheTarget = false;
            StartCoroutine(GiveAttackOrders(target));//give orders to guns to rotate towards the player to shoot
        }
    }

    private IEnumerator GiveAttackOrders(Transform target)
    {
        for (int i = 0; i < creatureWeapons.Length; i++)
        {
            if (!IsDead)
            {
                creatureWeapons[i].PrepareToShoot(target);//let the player prepare themslevs to shoot(to rotates towards the player)
                yield return new WaitForSeconds(secondsBetweenRightAndLeftGunsShots);//make delay between the right gun shot and the left gun shot
            }
        }
    }

    public void AttackUsingGun()//Function to call when you want the creature to shoot at the player
    {
        hasToLookAtTheTarget = true;//allow the rotating towards the player
    }

    public void OrderToStopShooting()//function to stop the creature from shooting at the player
    {
        for (int i = 0; i < creatureWeapons.Length; i++)
        {
            creatureWeapons[i].StopShooting();//orders guns to stop shooting
        }
    }


    private IEnumerator UpdateCreatureSpeed()
    {
        if (EnemyType == EnemyType.Flying)
        {
            EnemySpeed /= 4;
            yield return new WaitForSeconds(.5f);
            EnemySpeed *= 4;
        }
        else
        {
            agent.speed /= 4;
            yield return new WaitForSeconds(.5f);
            agent.speed *= 4;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Constants.projectileLayerNumber)//if a projectile hit it
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile.WasShoot)
            {
                ReceiveDamageFromObject(projectile.DamageCost, hitForce, other.transform.position);
            }
        }

        if (other.CompareTag(Constants.SecuritySensor))//if it entred a security area of the security weapon
        {
            isInsideSeacurityArea = true;
        }

        if (other.CompareTag(Constants.SecuritySensor))//if this creature entered a security sensor area
        {
            SecuritySensor securitySensor = other.GetComponent<SecuritySensor>();

            if (!securitySensor.IsItAirPlaneSensor)//if this sensore is a gun sensor
            {
                if ((securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.ground && EnemyType == EnemyType.Grounded) || (securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.air && EnemyType == EnemyType.Flying))
                {
                    if (!WasDied)
                    {
                        securitySensor.Targets.Add(this);//add it to the targets list of that security weapon
                    }
                }
            }
            else//if this sensor is a fighter airplane sensor
            {
                if (!WasDied)
                {
                    securitySensor.Targets.Add(this);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.SecuritySensor))//if it exited the security area of the security weapon
        {
            isInsideSeacurityArea = false;
        }

        if (other.CompareTag(Constants.SecuritySensor))//if this sensore is a gun sensor
        {
            SecuritySensor securitySensor = other.GetComponent<SecuritySensor>();

            if (!securitySensor.IsItAirPlaneSensor)
            {
                if ((securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.ground && EnemyType == EnemyType.Grounded) || (securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.air && EnemyType == EnemyType.Flying))
                {
                    securitySensor.Targets.Remove(this);

                    if (securitySensor.Target != null && this == securitySensor.Target)//if that target who exited the security area is the target that the weapon was shooting at him then stop it from shooting at him
                    {
                        securitySensor.SecurityWeapon.StopShooting();
                        securitySensor.Target = null;
                    }
                }
            }
            else//if this sensor is a fighter airplane sensor
            {
                securitySensor.Targets.Remove(this);

                if (securitySensor.Target != null && this == securitySensor.Target)//if that target who exited the security area is the target that the weapon was shooting at him then stop it from shooting at him
                {
                    securitySensor.FighterAirPlane.StopShooting();
                    securitySensor.Target = null;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (IsItBug)
        {
            EventsManager.onBossDie -= killBugs;
        }
    }
}