using System.Collections;
using System.Collections.Generic;
using ManagersAndControllers;
using Player;
using Projectiles;
using UnityEngine;

namespace Creatures {
    public interface IFlyingCreature { }

    public class FlyingCreature : Creature, IFlyingCreature {
        [HideInInspector]
        public bool IsItTheLeader;
        [HideInInspector]
        public bool IsItGroupMember;
        [HideInInspector]
        public Transform CreaturePointInGroup; //Point that where the member position should be in the group to go back to it when it has to form back

        [SerializeField] private float mingroupMemberSecondsToPatrolAgain; //min seconds that the air member has to wait before it starts patrolling again
        [SerializeField] private float maxgroupMemberSecondsToPatrolAgain; //max seconds that the air member has to wait before it starts patrolling again

        [SerializeField] private float smoothRotatingSpeed; //the speed of creature rotating 
        [SerializeField] private float patrollingSpeed; //speed of creature patrolling movement
        [SerializeField] private Vector3 patrolRange; //a range which consider how far the range between the pointToPatrolAround object postion and the randomPatrolPosiotion which are we going to generate
        [SerializeField]
        private Vector3 patrolUnWantedPositions; //positions between (-unWantedPositions and unWantedPositions) will add to them the patrolOffset
        [SerializeField]
        private Vector3 patrolOffset; //to prevent close position to the target when the air creature fly near ro it
        [SerializeField] private List<StinkyBall> stinkyBallPrefabs = new();
        [SerializeField] private Transform stinkyBallSpawnPoint;
        [SerializeField] private float stinkyBallSpeed;
        [SerializeField] private float stinkyBallGrowingTime; //how many seconds does the stinky ball takes to complete it's growing 
        [SerializeField] private AnimationClip stinkyBallGrowingAnimationClip;
        [SerializeField] private Transform BodyRig;
        private int airPointIndex; //index of the next targetWayPoint in the airWayPoints array
        private readonly List<Transform> airWayPoints = new(); //air points which the creature has to follow
        private Creature creature;
        private List<Paths> creaturePathes = new();
        private GameHandler gameHandler;
        private bool hasFinishedAttacking; //true if he finished attacking
        //private Transform pointToPatrolAround;//a point which takes the position of the enemy in the last way point he reach so after that he gonna patrol around this point using the randomPatrolPosiotion
        private bool hasToPatrol; //true if the creature reached the last way point and he has to patrol now
        private bool hasToRunAway; //true when one of the cinematic enemies dies(to let the air cinematic creature run away)
        private bool isAttacking; //true if the creature was ordered to attack
        private bool isFormingBack; //if the air creature going back to it's position in the group
        private int isNotTargetingPlayer; //0 if the target was the player, 1 if the target was a security weapon
        private bool isThrewingStinkyBall; //true if the creature is preparing the stinky ball to threw it brcause it needs time to get bigger(to grow it)
        private Transform leader; //the leader of the air creatures group
        private Transform nextCinematicPatrolPoint;
        private Transform nextTargetPoint; //nextTargetPoint is the next point that the creature is going to
        private ArcheryRig player;
        private Transform pointToAttackThePlayer; //the air creatures are gonna attack this point when they got an order to attack
        private Transform pointToLookAtThePlayer; //the air creatures are gonna look at this point when they are patrolling around
        private Transform pointToShootAtThePlayer; //the air creatures are gonna shoot at this point when they are patrolling around
        private int randAttackTypeNum; // 0 = threw stinky ball, 1 = Attack player, else = patrol to random point
        private Vector3 randomPatrolPosiotion; //a random position around the pointToPatrolAround object
        private Transform runAwayPoint;
        private Transform targetToFollow;
        private Vector3 wantedAngle, oldAngle; //wanted angle is the angle that the creature has to rotate to it to reach the wanted point, old angle is the current angle
        public List<Paths> CreaturePathes {
            set => creaturePathes = value;
        }

        /*private void Start() {
            animator = GetComponent<Animator>();
            hasToRunAway = false;
            gameHandler = FindObjectOfType<GameHandler>();
            player = FindObjectOfType<ArcheryRig>();

            if (!IsItGroupMember && !IsItTheLeader) {
                isNotTargetingPlayer = Random.Range(0, 2);
                EventsManager.onSecurityWeaponDestroy += FindTarget;
            }

            if (isNotTargetingPlayer == 1) FindTarget();

            creature = GetComponent<Creature>();
            isFormingBack = false;


            if (CompareTag(Constants.OnStartWaves)) EventsManager.onStartEnemyDeath += OrderToRunAway;

            if (IsItGroupMember && !IsItTheLeader) //if it was an air group member and not the leader
            {
                EventsManager.onAirLeaderPatrolling += OrderToPatrol;
                EventsManager.onAirLeaderDeath += BreakAirGroup;
            }

            spawner = FindObjectOfType<Spawner>();
            creatureColor = creature.CreatureColor;
            isThrewingStinkyBall = false;
            isAttacking = false;
            runAwayPoint = spawner.RunningAwayPoints[Random.Range(0, spawner.RunningAwayPoints.Length)]; //get a random run away point to go to

            waypoint[] pathWaypoints;

            if (CompareTag(Constants.OnStartWaves)) //if it was a cinematic creature
            {
                pathWaypoints = spawner.AirCinematicEnemyWaypoints.ToArray(); //get the air cinematic waypoints
                airPointIndex = Random.Range(0, pathWaypoints.Length); //ge random point index to go to
            } else //if it was not a cinematic enemy
            {
                pathWaypoints = creaturePathes[Random.Range(0, creaturePathes.Count)].Waypoints.ToArray(); //get a random path from the available pathes in that spawn point
                pointToAttackThePlayer = GameObject.FindWithTag(Constants.PlayerAttackPoint).transform;
                pointToLookAtThePlayer = GameObject.FindWithTag(Constants.PlayerLookAtPoint).transform;
                pointToShootAtThePlayer = GameObject.FindWithTag(Constants.PlayerShootAtPoint).transform;
                airPointIndex = 0;
            }

            for (int i = 0; i < pathWaypoints.Length; i++) airWayPoints.Add(pathWaypoints[i].transform); //get the transorms of these way points

            hasToPatrol = false;

            nextTargetPoint = airWayPoints[airPointIndex]; //creature takes the first point in the array 
            nextCinematicPatrolPoint = airWayPoints[Random.Range(0, airWayPoints.Count)]; //get a random patrol air point
        }*/

        /*private void Update() {
            if (creature.CurrentState != Creature.CreatureState.Dead) {
                if (CompareTag(Constants.OnStartWaves)) {
                    if (!hasToRunAway) {
                        RotateToTheWantedAngle(nextCinematicPatrolPoint);
                        PatrolCinematicCreature();
                    } else {
                        RotateToTheWantedAngle(runAwayPoint);
                        RunAway();
                    }
                } else {
                    if (isNotTargetingPlayer == 0) {
                        RotateToTheWantedAngle(pointToLookAtThePlayer);
                        FollowTarget(pointToAttackThePlayer);
                    } else {
                        RotateToTheWantedAngle(targetToFollow);
                        FollowTarget(targetToFollow);
                    }
                }
            }
        }*/

        /*private void LateUpdate() {
            if (creature.CurrentState != Creature.CreatureState.Dead)
                if (IsItGroupMember && isFormingBack) //if it was going back to it's position in the group then keep calculating it's point in gorup position so in case the leader moved so we still getting the updated position as soon as it's position depends on the leader position
                    randomPatrolPosiotion = CreaturePointInGroup.position;
        }*/

        private void OnDestroy() {
            if (CompareTag(Constants.OnStartWaves)) EventsManager.onStartEnemyDeath -= OrderToRunAway;

            if (IsItGroupMember && !IsItTheLeader) //if it was an air group member and not the leader
            {
                EventsManager.onAirLeaderPatrolling -= OrderToPatrol;
                EventsManager.onAirLeaderDeath -= BreakAirGroup;
                EventsManager.onSecurityWeaponDestroy -= FindTarget;
            }
        }

        public void FindTarget() {
            bool isThereTarget = false;

            for (int i = 0; i < gameHandler.SecurityWeapons.Length; i++) //find not destroied security weapon
                if (gameHandler.SecurityWeapons[i] != null && !gameHandler.SecurityWeapons[i].WasDestroyed) {
                    isThereTarget = true;
                    targetToFollow = gameHandler.SecurityWeapons[i].transform;
                }

            if (!isThereTarget) //if all the security weapons were destroied
            {
                if (player == null) player = FindObjectOfType<ArcheryRig>();

                targetToFollow = player.transform;
            }
        }

        /*public void FollowTarget(Transform target) {
            if (Mathf.Abs(Vector3.Distance(transform.position, target.position)) <= .2f && !hasToPatrol) //if the creature has reached the nextTargetPoint point && not patrolling
            {
                if (!CompareTag(Constants.OnStartWaves)) //if it was not a cinematic enemy
                {
                    if (IsItTheLeader) EventsManager.OnAirLeaderPatrolling(transform); //Order the group members to start patrolling

                    hasToPatrol = true;
                } else if (CompareTag(Constants.OnStartWaves)) //if it was a cinematic enemy
                {
                    if (!hasToRunAway) //if he reached it's target point and was not runnig away
                        nextTargetPoint = airWayPoints[Random.Range(0, airWayPoints.Count)]; //get a random patrol air point
                }
            }

            if (hasToPatrol) //if it has to patrol around
            {
                if (isNotTargetingPlayer == 1) StartPatrolling(target);
                else StartPatrolling(pointToAttackThePlayer);
            } else {
                if (!IsItGroupMember || IsItTheLeader) transform.position = Vector3.Lerp(transform.position, target.position, Speed * Time.deltaTime / Vector3.Distance(transform.position, target.position)); //make the creature moves
            }
        }*/


        //public void FollowAirWayPoints()
        //{
        //    if (Mathf.Abs(Vector3.Distance(transform.position, nextTargetPoint.position)) <= .5f && !hasToPatrol)//if the creature has reached the nextTargetPoint point && not patrolling
        //    {
        //        if (!CompareTag(Constants.OnStartWaves))//if it was not a cinematic enemy
        //        {
        //            if (airPointIndex + 1 < airWayPoints.Count)//if the next index is not out of range of the array
        //            {
        //                airPointIndex++;//get the next point index
        //                nextTargetPoint = airWayPoints[airPointIndex];
        //            }
        //            else
        //            {
        //                if (IsItTheLeader)
        //                {
        //                    EventsManager.OnAirLeaderPatrolling(transform);//Order the group members to start patrolling
        //                }

        //                hasToPatrol = true;
        //            }
        //        }
        //        else if (CompareTag(Constants.OnStartWaves))//if it was a cinematic enemy
        //        {
        //            if (!hasToRunAway)//if he reached it's target point and was not runnig away
        //            {
        //                nextTargetPoint = airWayPoints[Random.Range(0, airWayPoints.Count)];//get a random patrol air point
        //            }
        //            else//if cinematic creature was running away and reached it's run away point then it has to die
        //            {
        //                creature.Suicide();//die
        //            }
        //        }
        //    }

        //    if (hasToPatrol)//if it has to patrol around
        //    {
        //        StartPatrolling(nextTargetPoint);
        //    }
        //    else
        //    {
        //        if (!IsItGroupMember || IsItTheLeader)
        //        {
        //            RotateToTheWantedAngle(nextTargetPoint);
        //            transform.position += transform.forward * creature.EnemySpeed * Time.deltaTime;//make the creature moves forward
        //        }
        //    }
        //}

        /// <summary>
        ///     To put the creature in the right rotation
        /// </summary>
        /// <param name="objectToLookAt">the object that you want the creature to look at while he is moving</param>
        private void RotateToTheWantedAngle(Transform objectToLookAt) {
            //I'm doing that as a trick to get the wanted angle and after that i'm resetting the angle to it's old angle and that because we need to rotates the creature smoothly and not suddenly which make it cooler
            oldAngle = transform.eulerAngles; //save old angle

            transform.LookAt(objectToLookAt); //look at the target
            wantedAngle = transform.eulerAngles; //get the wanted eural angle after he looked
            transform.eulerAngles = oldAngle; //reset the angle to the old angle 

            Quaternion newAngle = Quaternion.Euler(wantedAngle); //get the new angle from the wanted eural angle (needed for the next step)
            transform.rotation = Quaternion.Lerp(transform.rotation, newAngle, smoothRotatingSpeed * Time.deltaTime); //rotate the creature smoothly from old angle to the new one
        }

        private void StartPatrolling(Transform pointToLookAt) //this function is gonna keep the creature looking towrdes the player because in this case he is attacking the player or something
        {
            PatrolAround(pointToLookAt);
        }

        private void PatrolAround(Transform target) //this function is gonna let the player move from random position to another
        {
            if (randomPatrolPosiotion == Vector3.zero) //if we did not give him a point to patrol around yet
                GetNewPatrolPosition(target); //get a random position around that point

            if (!isAttacking && !isThrewingStinkyBall) {
                if (Mathf.Abs(Vector3.Distance(transform.position, randomPatrolPosiotion)) <= .2f) //if you reached that random position
                {
                    if (!isFormingBack) //if it was not going back to the group
                    {
                        randAttackTypeNum = Random.Range(0, 3);

                        if (randAttackTypeNum == 0) {
                            StartCoroutine(ThrewStinkyBall(target));
                        } else if (randAttackTypeNum == 1 && isNotTargetingPlayer == 0) {
                            Attack();
                        } else {
                            if (IsItGroupMember && !IsItTheLeader) //if it was a group member
                                OrderToFormBack(); //order it to go back to it's postition in the group 
                            else GetNewPatrolPosition(target); //get a new random position to go to
                        }
                    } else //if he was going back to the gorup and reached his position in the group then prevent him from patroling around for a while
                    {
                        hasToPatrol = false;
                        isFormingBack = false;
                        transform.parent = leader.transform; //make his parent the leader again to make sure they stay in the wanted shape and move acoording to it
                        StartCoroutine(OrderToPatrolWithDlay()); //make him patrol after a while
                    }
                } else {
                    if (isFormingBack) //if is going to it's position in the group the douple it's speed
                        transform.position = Vector3.Lerp(transform.position, randomPatrolPosiotion, patrollingSpeed * 2 * Time.deltaTime / Vector3.Distance(transform.position, randomPatrolPosiotion)); //move the creautre to the wanted random postion 
                    else transform.position = Vector3.Lerp(transform.position, randomPatrolPosiotion, patrollingSpeed * Time.deltaTime / Vector3.Distance(transform.position, randomPatrolPosiotion)); //move the creautre to the wanted random postion 
                }
            } else if (isAttacking && !isThrewingStinkyBall) {
                if (Mathf.Abs(Vector3.Distance(transform.position, target.position)) > .2f && !hasFinishedAttacking) transform.position = Vector3.Lerp(transform.position, target.position, patrollingSpeed * Time.deltaTime / Vector3.Distance(transform.position, target.position)); //move the creautre to the player position
                else if (Mathf.Abs(Vector3.Distance(transform.position, target.position)) <= .2f && !hasFinishedAttacking) //if you has reached the player posistion and has not finished the attack
                    hasFinishedAttacking = true;

                if (hasFinishedAttacking) {
                    if (Mathf.Abs(Vector3.Distance(transform.position, randomPatrolPosiotion)) <= .2f) //if went back to it's old position after attacking the player
                    {
                        GetNewPatrolPosition(target); //get a new random position to go to
                        isAttacking = false;

                        if (IsItGroupMember && !IsItTheLeader) //if it was a group member
                            OrderToFormBack(); //order it to go back to it's postition in the group 
                    } else {
                        transform.position = Vector3.Lerp(transform.position, randomPatrolPosiotion, patrollingSpeed * Time.deltaTime / Vector3.Distance(transform.position, randomPatrolPosiotion)); //move the creautre to the wanted random postion
                    }
                }
            }
        }

        private void GetNewPatrolPosition(Transform pointToPatrolAround) //function to generate random positions around that point
        {
            randomPatrolPosiotion.x = Random.Range(pointToPatrolAround.position.x - patrolRange.x, pointToPatrolAround.position.x + patrolRange.x);
            randomPatrolPosiotion.y = Random.Range(pointToPatrolAround.position.y, pointToPatrolAround.position.y + patrolRange.y);
            randomPatrolPosiotion.z = Random.Range(pointToPatrolAround.position.z - patrolRange.z, pointToPatrolAround.position.z);

            if (Mathf.Abs(randomPatrolPosiotion.x - pointToPatrolAround.position.x) < patrolUnWantedPositions.x && Mathf.Abs(randomPatrolPosiotion.x - pointToPatrolAround.position.x) > -patrolUnWantedPositions.x) {
                if (randomPatrolPosiotion.x < 0) randomPatrolPosiotion.x -= patrolOffset.x;
                else randomPatrolPosiotion.x += patrolOffset.x;
            }
            if (Mathf.Abs(randomPatrolPosiotion.y - pointToPatrolAround.position.y) < patrolUnWantedPositions.y && Mathf.Abs(randomPatrolPosiotion.y - pointToPatrolAround.position.y) > -patrolUnWantedPositions.y) {
                if (randomPatrolPosiotion.y < 0) randomPatrolPosiotion.y -= patrolOffset.y;
                else randomPatrolPosiotion.y += patrolOffset.y;
            }
            if (Mathf.Abs(randomPatrolPosiotion.z - pointToPatrolAround.position.z) < patrolUnWantedPositions.z && Mathf.Abs(randomPatrolPosiotion.z - pointToPatrolAround.position.z) > -patrolUnWantedPositions.z) randomPatrolPosiotion.z -= patrolOffset.z;
        }

        public IEnumerator ThrewStinkyBall(Transform target) {
            isThrewingStinkyBall = true;
            foreach (StinkyBall sB in stinkyBallPrefabs)
                if (sB.StinkyBallColor == creatureColor) //find the stinky ball that matchs the creature color
                {
                    GameObject stinkyBall = Instantiate(sB.gameObject, stinkyBallSpawnPoint.position, transform.rotation);

                    if (stinkyBall != null) stinkyBall.transform.parent = BodyRig.transform; //make the stinky ball parent is the BodyRig of the creature who created it

                    var stinkyAnimator = GetComponent<Animator>();
                    stinkyAnimator.speed = stinkyBallGrowingAnimationClip.length / stinkyBallGrowingTime;
                    stinkyAnimator.Play(Constants.StinkyBallGrowingAnimation);

                    yield return new WaitForSeconds(stinkyBallGrowingTime); //wait untill the stinky ball growing animation finishs

                    if (stinkyBall != null) {
                        stinkyBall.transform.parent = null; //unparent the stinky ball after threwing it
                        //animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.CastSpell));
                        yield return new WaitForSeconds(.25f); //wait untill the creature play the cast spell animation
                        if (stinkyBall != null) {
                            if (isNotTargetingPlayer == 1) stinkyBall.GetComponent<StinkyBall>().FollowTarget(target);
                            else stinkyBall.GetComponent<StinkyBall>().FollowTarget(pointToShootAtThePlayer);
                        }
                    }

                    yield return new WaitForSeconds(2); //wait for 2 seconds after threwing the stinky ball

                    isThrewingStinkyBall = false;

                    if (IsItGroupMember && !IsItTheLeader) //if it was a group member
                        OrderToFormBack(); //order it to go back to it's postition in the group 

                    GetNewPatrolPosition(target); //get a new random position to go to
                    break;
                }
        }

        /// <summary>
        ///     called when a slow time arrow get activated
        /// </summary>
        /// <param name="speedDivider">number to change the patrolling speed according to it</param>
        /// <param name="hasToSlowDown">true = has to slow down, false = has to back to normal patrolling speed</param>
        public void SetPatrollingSpeed(float speedDivider, bool hasToSlowDown) {
            if (hasToSlowDown) patrollingSpeed /= speedDivider;
            else patrollingSpeed *= speedDivider;
        }

        public void Attack() {
            isAttacking = true;
            hasFinishedAttacking = false;
        }

        public void OrderToRunAway() //to let the air cinematic creature run away when one of the cinematics enemies dies
        {
            hasToRunAway = true;
        }

        /*public void RunAway() {
            if (Mathf.Abs(Vector3.Distance(transform.position, runAwayPoint.position)) <= .2f) //if the cinematic creature has reached the runAwayPoint point
            {
                hasToRunAway = false;
                creature.Suicide(); //die
            } else {
                RotateToTheWantedAngle(runAwayPoint);
                transform.position = Vector3.Lerp(transform.position, runAwayPoint.position, Speed * Time.deltaTime / Vector3.Distance(transform.position, runAwayPoint.position)); //make the creature moves
                //transform.position += transform.forward * creature.EnemySpeed * Time.deltaTime;//make the creature moves forward
            }
        }*/

        public void OrderToPatrol(Transform leader = null) //usually used for group members
        {
            if (leader != null) this.leader = leader;

            transform.parent = null; //free him from the leader so he can move seperatly
            hasToPatrol = true; //let him patrol around
        }

        /// <summary>
        ///     to calc his position in the group and ask him to go back to it by assigning the patrol point to that position
        /// </summary>
        public void OrderToFormBack() {
            randomPatrolPosiotion = CreaturePointInGroup.position;
            isFormingBack = true;
        }

        public IEnumerator OrderToPatrolWithDlay() {
            yield return new WaitForSeconds(Random.Range(maxgroupMemberSecondsToPatrolAgain, mingroupMemberSecondsToPatrolAgain));
            OrderToPatrol();
        }

        public void BreakAirGroup() //to stop letting the air group members from forming a group
        {
            transform.parent = null; //unparent the members from the leader
            IsItGroupMember = false;
            IsItTheLeader = false;
            isFormingBack = false;
            StopCoroutine(OrderToPatrolWithDlay());
        }
    }
}