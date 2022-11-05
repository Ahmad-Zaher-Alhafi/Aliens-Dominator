using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arrows;
using Collectables;
using Creatures.Animators;
using Defence_Weapons;
using ManagersAndControllers;
using Pool;
using Projectiles;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Creatures {
    public abstract class Creature : PooledObject {
        public enum CreatureType {
            Grounded,
            Flying
        }

        public enum CreatureState {
            Idle,
            Patrolling,
            FollowingPath,
            GettingHit,
            Attacking,
            Chasing,
            Dead
        }

        public enum CreatureAction {
            StayIdle,
            Patrol,
            FollowPath,
            GetHit,
            Attack,
            Chase,
            Die
        }

        public string EnemyId { get; private set; }
        public int ScorePoints = 100;

        public CreatureType Type = CreatureType.Grounded;
        public CreatureState CurrentState;
        public CreatureState PreviousState;

        [SerializeField] private List<Material> colors;
        [SerializeField] private float slowdownTimer = 6f;
        [SerializeField] private float attackDamage = 10f;

        [SerializeField] private GameObject CreatureStateCanves;
        [SerializeField] private Slider CreatureHealthBar;

        [SerializeField] private float hitForce = 1000;
        [SerializeField] private float health = 100f;

        [SerializeField] private List<TextMotionBehavior> textsMotionBehavior;
        [SerializeField] private int chanceOfDroppingArrow;

        [SerializeField] private int waypointMinIndexToActivateSpecialActions = 2; // We dont want the enemies to start shooting from the spawn waypoint
        [SerializeField] private float timeToDestroyDeadBody = 3; //time needed before destroying the dead body of the creature
        [SerializeField] protected Constants.ObjectsColors creatureColor;
        public GameObject RigBody; //the RigMain object that is inside the creature object

        [SerializeField] private float secondsBetweenRightAndLeftGunsShots; //time between the right bullet from the right gun and the left bullet from the left gun

        public float AttackTimer = 3f;

        [HideInInspector]
        public string AttackerId;
        private NavMeshAgent agent;
        private AudioSource audioSource;
        private Rigidbody body;
        private Rigidbody[] ChildrenRigidbody;

        private GameHandler GameHandler;
        private bool hasToLookAtTheTarget; //if he has to rotates towards the target to start shooting
        private float initialHealth;
        private bool isInsideSeacurityArea; //true if it entred the security area of the security weapon

        private Transform playerLookAtPoint; //point where the creature canves is gonna look at
        private Transform playerShootAtPoint; //point where the creature is gonna look at to shoot
        private Rigidbody rig;
        private Coroutine slowdownCoroutine;
        protected CreatureSpawnController creatureSpawnController;
        public CreatureMover CreatureMover { get; private set; }
        protected CreatureAnimator animator;
        [SerializeField] private float applyDamageWhenHypnotized = 5f;
        public bool IsSlowedDown { get; set; }
        public Constants.ObjectsColors CreatureColor => creatureColor;

        private void Awake() {
            CreatureMover = GetComponent<CreatureMover>();
        }

        private void FixedUpdate() {
            if (CreatureMover.IsBusy || CurrentState == CreatureState.Dead) return;

            PreviousState = CurrentState;
            CurrentState = GetRandomActionToDo() switch {
                CreatureAction.StayIdle => CreatureState.Idle,
                CreatureAction.Patrol => CreatureState.Patrolling,
                _ => CurrentState
            };

            /*if (this is IFightingCreature && hasToLookAtTheTarget) //if this creature has guns and has to start rotating towards the player
                LookAtTheTarget(playerShootAtPoint);

            CreatureStateCanves.transform.LookAt(playerLookAtPoint);*/
        }

        private CreatureAction GetRandomActionToDo() {
            int randomNumber = Random.Range(0, 2);
            return randomNumber == 0 ? CreatureAction.StayIdle : CreatureAction.Patrol;
        }

        private void OnDestroy() {
            if (this is IBugCreature) EventsManager.onBossDie -= killBugs;
        }

        /// <summary>
        ///     This is for hypnotized enemies, so they can't kill all enemies (basically balancing)
        /// </summary>
        /// <returns></returns>
        private IEnumerator DecreaseHealth() {
            yield return new WaitForSeconds(5f);

            if (CurrentState != CreatureState.Dead) {
                health -= applyDamageWhenHypnotized;

                if (health <= 0f) {
                    Suicide();
                    Debug.Log("Enemy died due to hypnotization");
                } else {
                    StartCoroutine(DecreaseHealth());
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.layer == Constants.PROJECTILE_LAYER_ID) //if a projectile hit it
            {
                var projectile = other.GetComponent<Projectile>();
                if (projectile.WasShoot) ReceiveDamageFromObject(projectile.DamageCost, hitForce, other.transform.position);
            }

            if (other.CompareTag(Constants.SecuritySensor)) //if it entred a security area of the security weapon
                isInsideSeacurityArea = true;

            if (other.CompareTag(Constants.SecuritySensor)) //if this creature entered a security sensor area
            {
                var securitySensor = other.GetComponent<SecuritySensor>();

                if (!securitySensor.IsItAirPlaneSensor) //if this sensore is a gun sensor
                {
                    if (securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.ground && Type == CreatureType.Grounded || securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.air && Type == CreatureType.Flying)
                        if (CurrentState != CreatureState.Dead)
                            securitySensor.Targets.Add(this); //add it to the targets list of that security weapon
                } else //if this sensor is a fighter airplane sensor
                {
                    if (CurrentState != CreatureState.Dead) securitySensor.Targets.Add(this);
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag(Constants.SecuritySensor)) //if it exited the security area of the security weapon
                isInsideSeacurityArea = false;

            if (other.CompareTag(Constants.SecuritySensor)) //if this sensore is a gun sensor
            {
                var securitySensor = other.GetComponent<SecuritySensor>();

                if (!securitySensor.IsItAirPlaneSensor) {
                    if (securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.ground && Type == CreatureType.Grounded || securitySensor.SecurityWeapon.weaponType == Constants.SecurityWeaponsTypes.air && Type == CreatureType.Flying) {
                        securitySensor.Targets.Remove(this);

                        if (securitySensor.Target != null && this == securitySensor.Target) //if that target who exited the security area is the target that the weapon was shooting at him then stop it from shooting at him
                        {
                            securitySensor.SecurityWeapon.StopShooting();
                            securitySensor.Target = null;
                        }
                    }
                } else //if this sensor is a fighter airplane sensor
                {
                    securitySensor.Targets.Remove(this);

                    if (securitySensor.Target != null && this == securitySensor.Target) //if that target who exited the security area is the target that the weapon was shooting at him then stop it from shooting at him
                    {
                        securitySensor.FighterAirPlane.StopShooting();
                        securitySensor.Target = null;
                    }
                }
            }
        }

        public void Init(Vector3 position, string enemyID) {
            CurrentState = CreatureState.Idle;
            transform.position = position;
            EnemyId = enemyID;

            if (this is IBugCreature) EventsManager.onBossDie += killBugs;

            CreatureStateCanves.SetActive(true);
            gameObject.SetActive(true);

            rig = GetComponent<Rigidbody>();
            rig.isKinematic = false;
            rig.useGravity = false;
            
            isInsideSeacurityArea = false;
            playerShootAtPoint = GameObject.FindGameObjectWithTag(Constants.PlayerShootAtPoint).transform;
            playerLookAtPoint = GameObject.FindGameObjectWithTag(Constants.PlayerLookAtPoint).transform;

            hasToLookAtTheTarget = false;
            IsSlowedDown = false;
            AttackerId = null;

            GameHandler = FindObjectOfType<GameHandler>();

            audioSource = GetComponent<AudioSource>();
            body = GetComponent<Rigidbody>();

            animator = GetComponent<CreatureAnimator>();
            animator.Init();
            CreatureMover.Init();
            creatureSpawnController = FindObjectOfType<CreatureSpawnController>();

            ChildrenRigidbody = GetComponentsInChildren<Rigidbody>();

            //EnableRagdoll(false, null);
            initialHealth = health;
            CreatureHealthBar.maxValue = initialHealth;
            CreatureHealthBar.minValue = 0;
            CreatureStateCanves.SetActive(false);
        }

        public void EnableRagdoll(bool enabled, GameObject hit, float force = 150f) {
            for (int i = 0; i < ChildrenRigidbody.Length; i++)
                if (!ChildrenRigidbody[i].CompareTag("BloodEffect")) {
                    ChildrenRigidbody[i].isKinematic = !enabled;
                    ChildrenRigidbody[i].useGravity = enabled;

                    var colCatcher = ChildrenRigidbody[i].GetComponent<CreatureCollider>();
                    if (!colCatcher) ChildrenRigidbody[i].gameObject.AddComponent<CreatureCollider>().InitializeCollider(this);
                    else colCatcher.InitializeCollider(this);

                    if (enabled) ChildrenRigidbody[i].AddExplosionForce(force, ChildrenRigidbody[i].transform.position, 3f);
                }

            body.detectCollisions = !enabled;
            body.isKinematic = !enabled;

            if (agent) agent.enabled = !enabled;

            if (Type == CreatureType.Grounded)


                if (hit == null)
                    return;

            for (int j = 0; j < ChildrenRigidbody.Length; j++) {
                if (!ChildrenRigidbody[j].CompareTag("BloodEffect")) {
                    ChildrenRigidbody[j].AddForce(hit.transform.forward * hitForce + Vector3.up * hitForce / 4);
                }
            }

            rig.isKinematic = enabled;
            rig.useGravity = !enabled;
        }


        public void GetHurt(IDamager damager, float damageWeight) {
            if (CurrentState == CreatureState.Dead) return;

            float totalDamage = damager.Damage * damageWeight;
            health -= totalDamage;
            PreviousState = CreatureState.GettingHit;
            ApplyDamage(damager.GameObject, hitForce);

            //ShowDamageText(totalDamage);
        }

        private void ShowDamageText(float dmg) {
            if (!CreatureStateCanves.gameObject.activeInHierarchy) CreatureStateCanves.SetActive(true);

            bool hasFoundFreeText = false;

            foreach (TextMotionBehavior text in textsMotionBehavior) //find a free text
                if (!text.IsItBusy) {
                    hasFoundFreeText = true;
                    text.ShowDamageTakenText(dmg);
                }

            /*if (!hasFoundFreeText) //if it has not found a free text then create new one
            {
                TextMotionBehavior newText = Instantiate(textsMotionBehavior[0], textsMotionBehavior[0].transform.parent);
                textsMotionBehavior.Add(newText);
                newText.ShowDamageTakenText(dmg);
            }*/
        }

        public void ReceiveExplosionDamage(ArrowBase arrow, float dmg, float force) {
            if (CurrentState == CreatureState.Dead) return;

            health -= dmg;

            PreviousState = CreatureState.GettingHit;
            ApplyDamage(arrow.gameObject, force);
        }

        public void ReceiveDamageFromObject(float dmg, float force, Vector3 damagedPosition = new()) {
            if (CurrentState == CreatureState.Dead) return;

            health -= dmg;
            PreviousState = CreatureState.GettingHit;

            ApplyDamage(null, force);
        }

        public void ReceiveDamageFromEnemy(GameObject enemy, float dmg, float force) {
            if (CurrentState == CreatureState.Dead) return;

            PreviousState = CreatureState.GettingHit;

            health -= dmg;

            if (AttackerId == null) {
                //Debug.Log("Fight back");

                if (!agent) return;

                agent.enabled = true;
                AttackerId = enemy.GetComponent<Creature>().EnemyId;
            }

            ApplyDamage(enemy, force);
        }

        private Coroutine Coroutine;
        //Apply damage to wall mainly for grounded enemies
        private IEnumerator AttackWall() {
            yield return new WaitForSeconds(3f);

            var wallManager = FindObjectOfType<WallManager>();

            //animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.Attack));
            wallManager.ReduceHealth(attackDamage);
            Coroutine = StartCoroutine(AttackWall());
        }

        public void Hypnotize(float bonusHealth) {
            if (CurrentState == CreatureState.Dead) return;

            health += bonusHealth;
        }

        /// <summary>
        ///     Revive the enemy
        /// </summary>
        /// <param name="bonusHealth"></param>
        /// <param name="bodyPoint">the point where the arrow hit the body</param>
        public void BringBackToLife(float bonusHealth, Transform bodyPoint) {
            //print("Back To Life");
            //animator.enabled = true;
            if (CurrentState != CreatureState.Dead || Type != CreatureType.Grounded) return;

            transform.position = bodyPoint.position;
            //transform.rotation = Quaternion.identity;
            EnableRagdoll(false, gameObject);
            //Reset the agent, otherwise it wont detect the NavMesh
            //if (agent && !agent.isOnNavMesh)
            //{
            agent.enabled = false;
            agent.enabled = true;
            //}

            CurrentState = CreatureState.Idle;
            creatureSpawnController.Ids.Add(EnemyId);
            GameHandler.AllEnemies.Add(gameObject);
            health = 100 + bonusHealth;
        }

        public void Suicide() {
            if (CurrentState == CreatureState.Dead) return;

            health = 0f;
            ApplyDamage(null, 100f);
        }

        /// <summary>
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="force"></param>
        /// <param name="damagedPosition">
        ///     it's the position where the arrow hit the creature if it was hit by an arrow otherwise
        ///     it's null and blood will be in the meddle of the creautre
        /// </param>
        private void ApplyDamage(GameObject hit, float force) {
            if (!CreatureStateCanves.gameObject.activeInHierarchy) {
                CreatureStateCanves.SetActive(true);
            }

            CreatureHealthBar.normalizedValue = health / initialHealth;

            if (health <= 0f) {
                PlayDeathSound(); //play creature death sound

                // TODO: Take a look at this
                //if it was inside the security area then call an event to remove it from the targets list of the security sensor
                if (isInsideSeacurityArea) {
                    EventsManager.OnEnemyDiesInsideSecurityArea(this);
                }

                if (!GameHandler.WasCinematicCreatuerDied && CompareTag("OnStartWaves")) {
                    GameHandler.WasCinematicCreatuerDied = true;
                    GameHandler.CinematicEnemies.Remove(gameObject.GetComponent<GroundCreatureMover>()); //remove the dead creature from the list
                    EventsManager.OnStartEnemyDeath(); //call an event that says one of the start enemies was died
                }

                int rand = Random.Range(1, 101);
                if (rand <= chanceOfDroppingArrow) SpawnBallon();

                creatureSpawnController.Ids.Remove(EnemyId);

                EnableRagdoll(true, hit);

                if (hit) body.AddForce(hit.transform.forward * force);

                if (CompareTag("OnStartWaves")) {
                    timeToDestroyDeadBody += 5; //to give the air creature exestra time to fall down before it gets destroied
                } else {
                    if (Type == CreatureType.Flying && GetComponent<FlyingCreature>().IsItTheLeader) //if this creature was a air group leader
                        EventsManager.OnAirLeaderDeath(); //call an event to break the group and unparent the children from the leader
                }

                StartCoroutine(DestroyEnemyObject());

                // if (IsItBoss) EventsManager.OnBossDie();

                if (gameObject.tag != "OnStartWaves") {

                    /*var ragdollRewind = RigBody.GetComponent<RagdollRewind>();
                    ragdollRewind.StartRecording(1, .1f, Animator);*/



                    --creatureSpawnController.spawnedEnemy;
                    ++creatureSpawnController.killedEnemies;

                    GameHandler.creatureSpawnController.Score += ScorePoints;
//                    GameHandler.AllEnemies.RemoveAt(GameHandler.AllEnemies.FindIndex(e => e == gameObject));
                }

                CurrentState = CreatureState.Dead;
            }
        }

        private IEnumerator DestroyEnemyObject() {
            yield return new WaitForSeconds(timeToDestroyDeadBody);

            if (!CompareTag("OnStartWaves")) {
                CreatureStateCanves.SetActive(false);
                gameObject.SetActive(false);

                List<ArrowBase> _arrows = GetComponentsInChildren<ArrowBase>().ToList();

                //Make sure we remove all arrows from creature before despawning it
                foreach (ArrowBase _arrow in _arrows) {
                    _arrow.transform.SetParent(null);
                    _arrow.gameObject.SetActive(false);

                    GameHandler.PoolManager.AddArrowToPool(_arrow.gameObject);
                }
            }

            ReturnToPool();
        }

        private void SpawnBallon() {
            Instantiate(GetBallon().gameObject, transform.position + Vector3.up, Quaternion.identity);
        }

        /*We need to generate a list of the correct amount of items (like if one item = 75%, there will be 75 items in the list (since I set max to 100)). 
    * Then it picks a random number and returns the object
    * Side note: caching this could cause some items to never get selected.
    * Note: Make sure all chances together = 100% otherwise it could return null
    */
        private Ballon GetBallon(int max = 100) {
            var list = new List<Ballon>();

            GameHandler.Ballons.ForEach(b => {
                float chance = Mathf.RoundToInt(max / 100 * b.ChanceOfSpawning);

                for (int i = 0; i < chance; i++) list.Add(b);
            });

            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// function to play creature death sound when it dies
        /// </summary>
        private void PlayDeathSound() {
            /*if (CompareTag("OnStartWaves")) //play scream sound if it was the first enemy so after that the army of creatures start attacking you
                AudioManager.Instance.PlayScreamSound();*/

            audioSource.Play();
        }

        private void killBugs() {
            Suicide();
        }

        private void LookAtTheTarget(Transform target) //to make the player look toward the target (the player for now)
        {
            Quaternion oldQuat = transform.rotation;
            transform.LookAt(target);
            float targetYAngle = transform.rotation.y;
            transform.rotation = oldQuat;
            var targetAngle = new Quaternion(transform.rotation.x, targetYAngle, transform.rotation.z, transform.rotation.w);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, .05f);

            if (Quaternion.Angle(transform.rotation, targetAngle) <= .2f) //if the creature finished rotating to the correct angle wihch is the player angle
            {
                hasToLookAtTheTarget = false;
                //StartCoroutine(GiveAttackOrders(target)); //give orders to guns to rotate towards the player to shoot
            }
        }

        /*private IEnumerator GiveAttackOrders(Transform target) {
            for (int i = 0; i < creatureWeapons.Length; i++)
                if (CurrentState != CreatureState.Dead) {
                    creatureWeapons[i].PrepareToShoot(target); //let the player prepare themslevs to shoot(to rotates towards the player)
                    yield return new WaitForSeconds(secondsBetweenRightAndLeftGunsShots); //make delay between the right gun shot and the left gun shot
                }
        }*/

        public void AttackUsingGun() //Function to call when you want the creature to shoot at the player
        {
            hasToLookAtTheTarget = true; //allow the rotating towards the player
        }

        /*public void OrderToStopShooting() //function to stop the creature from shooting at the player
        {
            for (int i = 0; i < creatureWeapons.Length; i++) creatureWeapons[i].StopShooting(); //orders guns to stop shooting
        }*/

        public void GotHit() {
            if (slowdownCoroutine != null) {
                StopCoroutine(slowdownCoroutine);
            }

            slowdownCoroutine = StartCoroutine(SlowDown());
        }

        private IEnumerator SlowDown() {
            IsSlowedDown = true;
            yield return new WaitForSeconds(slowdownTimer);
            IsSlowedDown = false;
        }
    }
}