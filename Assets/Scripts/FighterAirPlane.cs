using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FighterAirPlane : MonoBehaviour
{
    [HideInInspector]public bool HasToDefend;
    [HideInInspector] public bool IsShooting;
    [HideInInspector] public bool HasToUseRockets;//true then use rockets, false then use bullets

    [SerializeField] private List<Transform> airWayPoints = new List<Transform>();//air points which the creature has to follow
    [SerializeField] private float patrolSpeed;//speed of moveming between the airplane wayoints
    [SerializeField] private float animatingSpeed;//speed of going up and down in animating phase
    [SerializeField] private float takeOffSpeed;//speed of taking off
    [SerializeField] private float smoothRotatingSpeed;//speed of rotating towards a point
    [SerializeField] private Transform pointToLookAt;//point to look at if there was no target to look at
    [SerializeField] private float secondsBetwennPatrols;//secnods to wait before going to next waypoint
    [SerializeField] private float heightOnAnimation;//how much hight should the airplane should move aup and down on the y axis
    [SerializeField] private Transform landingPoint;//the point where the airplane should back to it before it lands(this point is above the airplane base)
    [SerializeField] private GameObject airplaneRocketPrefab;
    [SerializeField] private List<Projectile> airRockets = new List<Projectile>();//rockets of the airplane
    [SerializeField] private int maxBulletsNumber;
    [SerializeField] private int maxRocketsNumber;
    [SerializeField] private Transform ammoStateCanves;
    [SerializeField] private GameObject projectilePrefab;//bullet to threw
    [SerializeField] private Transform projectileCreatPoint;//bullet creat position
    [SerializeField] private float secondsBetweenEachProjectile;
    [SerializeField] private float secondsBetweenEachRocket;
    [SerializeField] private SecuritySensor securitySensor;
    [SerializeField] private float aimAccuracy;//the bigger the number the less the rotating accuracy which let the airplane shoot even before reaching the exact target angle(becasue the airplane shoot at the target after it rotates towads the target)
    [SerializeField] private TextMeshProUGUI rocketsAmmoStateText;
    [SerializeField] private TextMeshProUGUI bulletsAmmoStateText;
    [SerializeField] private ParticleSystem[] smokeParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Sound takeOffSound;
    [SerializeField] private Sound landSound;
    [SerializeField] private Sound bulletSound;
    [SerializeField] private GameObject weaponFixImg;
    [SerializeField] private GameObject updateWeaponFireRateImg;
    [SerializeField] private GameObject updateWeaponStrengthImg;

    private Transform nextTargetPoint;//nextTargetPoint is the next point that the airplane is going to
    private int airPointIndex;//index of the next targetWayPoint in the airWayPoints array
    private bool hadTakenOff;
    private bool hasToAnimate;
    private bool isPatrolling;
    private Vector3 upAnimatingPoint;//up animating point where the airplane is gonna use it to animate up
    private Vector3 downAnimatingPoint;//dwon animating point where the airplane is gonna use it to animate dwon
    private Vector3 nextAnimatingPoint;//to switch between (upAnimatingPoint && downAnimatingPoint)
    private bool hasToLand;
    private bool isGoingBackToBase;
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private int currentBulletsNumber;
    private int currentRocketsNumber;
    private Transform target;
    private Coroutine shootCoroutine;
    private List<AirRocketsReloadPoint> airRocketsReloadPoints = new List<AirRocketsReloadPoint>();
    private Transform playerPointToLookAt;
    private bool hasToPlayTakeOffSound;
    private bool hasToPlayLandSound;
    private bool isItTimeToShootAgain;//true if the airplane waited for a time before shooting towardes all the creatures again(needed only when using rockets to prevent shooting all the rockets at the same creatures)

    [System.Serializable]
    private class Sound
    {
        public AudioClip audioClip;
        public float volume;
    }

    private class AirRocketsReloadPoint
    {
        public Transform Parent;
        public Vector3 InitialLocalPosition;

        public AirRocketsReloadPoint(Transform parent, Vector3 initialLocalPosition)
        {
            this.Parent = parent;
            this.InitialLocalPosition = initialLocalPosition;
        }
    }

    void Start()
    {
        isItTimeToShootAgain = true;
        hasToPlayTakeOffSound = true;
        hasToPlayLandSound = true;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        isGoingBackToBase = false;
        hasToLand = false;
        isPatrolling = false;
        hasToAnimate = false;
        HasToDefend = false;
        hadTakenOff = false;
        airPointIndex = 0;
        nextTargetPoint = airWayPoints[0];

        playerPointToLookAt = GameObject.FindGameObjectWithTag(Constants.PlayerAttackPoint).transform;
        currentBulletsNumber = maxBulletsNumber;
        currentRocketsNumber = maxRocketsNumber;
        UpdateAmmoStateText(false);//update bullets number text
        UpdateAmmoStateText(true);//update rockets number text


        for (int i = 0; i < airRockets.Count; i++)
        {
            airRocketsReloadPoints.Add(new AirRocketsReloadPoint(airRockets[i].transform.parent.transform, airRockets[i].transform.localPosition));
        }

        EventsManager.onTakingAmmo += ReloadWeapon;
        EventsManager.onLevelFinishs += GoBackToBase;
        EventsManager.onLevelFinishs += ShowUpdateImages;
        EventsManager.onLevelStarts += HideUpdateImages;

        IsShooting = false;
        HasToUseRockets = false;
    }


    void Update()
    {
        ammoStateCanves.LookAt(playerPointToLookAt);

        if (HasToDefend)
        {
            if (!hadTakenOff)
            {
                TakeOff();
            }
            else if (!isPatrolling && !hasToLand)
            {
                isPatrolling = true;
                StartCoroutine(PatrollAround());
            }
            else if (hasToAnimate)
            {
                Animate();
            }
            else if (hasToLand)
            {
                Land();
            }

            if (!hasToLand)
            {
                if (target != null)//if there was a target to look at
                {
                    RotateToTheWantedAngle(target, true);
                }
                else//look at a point in the scene if there was no target 
                {
                    RotateToTheWantedAngle(pointToLookAt, false);
                }
            }
        }

        if (HasToDefend && !audioSource.isPlaying)//this code needed to loop the sound manually because i'm using the PlayOneShot() function and this one does not loop the sound and i can not use Play() function because the clip sound is so low, so when we get good clip we can remove this code
        {
            hasToPlayTakeOffSound = true;
            PlayTakeOffSound();
        }
    }

    private void ShowUpdateImages()
    {
        weaponFixImg.SetActive(true);
        updateWeaponFireRateImg.SetActive(true);
        updateWeaponStrengthImg.SetActive(true);

    }

    private void HideUpdateImages()
    {
        weaponFixImg.SetActive(false);
        updateWeaponFireRateImg.SetActive(false);
        updateWeaponStrengthImg.SetActive(false);
    }

    public void Defend()
    {
        PlayTakeOffSound();
        HasToDefend = true;
        
        if (hasToLand)
        {
            hasToLand = false;
            hadTakenOff = false;
        }

        isGoingBackToBase = false;

        airPointIndex = 0;
        nextTargetPoint = airWayPoints[0];
        for (int i = 0; i < smokeParticles.Length; i++)
        {
            smokeParticles[i].Play();
        }
    }

    public IEnumerator PatrollAround()
    {
        while (HasToDefend && !hasToLand && hadTakenOff)
        {
            if (Mathf.Abs(Vector3.Distance(transform.position, nextTargetPoint.position)) <= .5f)//if the airplane has reached the nextTargetPoint point
            {
                if (isGoingBackToBase)
                {
                    isGoingBackToBase = false;
                    hasToLand = true;
                    break;
                }
                else
                {
                    if (airPointIndex + 1 < airWayPoints.Count)//if the next index is not out of range of the array
                    {
                        airPointIndex++;//get the next point index
                    }
                    else
                    {
                        airPointIndex = 0;
                    }

                    nextTargetPoint = airWayPoints[airPointIndex];

                    upAnimatingPoint = transform.position + Vector3.up * heightOnAnimation;//set the upAnimatingPoint
                    downAnimatingPoint = transform.position + Vector3.down * heightOnAnimation;//set the downAnimatingPoint
                    nextAnimatingPoint = upAnimatingPoint;//let the animating starts with upAnimatingPoint

                    hasToAnimate = true;//order to animate untill the patroll seconds wait finish
                    yield return new WaitForSeconds(secondsBetwennPatrols);
                    hasToAnimate = false;//to stop animating and keep moving to next waypoint
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, nextTargetPoint.position, patrolSpeed * Time.deltaTime / Vector3.Distance(transform.position, nextTargetPoint.position));//move the airplane to the nextTargetPoint
                yield return new WaitForSeconds(.001f);
            }
        }
        isPatrolling = false;
    }

    /// <summary>
    /// To let the airplane look towards the objectToLookAt
    /// </summary>
    /// <param name="objectToLookAt">the object that you want the airplane to look at while he is moving</param>
    /// <param name="isItTarget">true if it is rotating towards a target (then it has to shoot after finishing rotating) else it's rotating towards some point(so it does not have to shoot after rotating)</param>
    private void RotateToTheWantedAngle(Transform objectToLookAt, bool isItTarget)
    {
        Quaternion wantedAngle = transform.rotation;

        if ((HasToUseRockets && !isItTarget) || !HasToUseRockets)//if it was using rockets and there were no target or if it was not using rockets(because using rockets does not need to look at the player because the air plane is gonna shoot all the target at once using multi rockets so no need to look at the targets)
        {
            //I'm doing that as a trick to get the wanted angle and after that i'm resetting the angle to it's old angle and that because we need to rotates the airplane smoothly and not suddenly which make it cooler
            Quaternion oldAngle = transform.rotation;//save old angle
            transform.LookAt(objectToLookAt);//look at the target
            wantedAngle = transform.rotation;//get the wanted eural angle after he looked
            transform.rotation = oldAngle;//reset the angle to the old angle 
            transform.rotation = Quaternion.Lerp(transform.rotation, wantedAngle, smoothRotatingSpeed / Quaternion.Angle(transform.rotation, wantedAngle));//rotate the airplane smoothly from old angle to the new one
        }

        if (isItTarget)
        {
            if (HasToUseRockets && !IsShooting)
            {
                IsShooting = true;
                shootCoroutine = StartCoroutine(Shoot(target));//give orders to airplane to rotate towards the target to shootCoroutine
            }
            else if (!HasToUseRockets && !IsShooting && Quaternion.Angle(transform.rotation, wantedAngle) <= aimAccuracy)//if the airplane finished rotating to the correct angle wihch is the target angle
            {
                IsShooting = true;
                shootCoroutine = StartCoroutine(Shoot(target));//give orders to airplane to rotate towards the target to shootCoroutine
            }
        }
    }

    private void TakeOff()
    {
        if (Mathf.Abs(transform.position.y - nextTargetPoint.position.y) >= .5f)
        {
            transform.position += Vector3.up * takeOffSpeed * Time.deltaTime;//make the airplane moves upwards
        }
        else
        {
            hadTakenOff = true;
        }
    }

    private void Animate()//to let the airplane move up and down in small movement
    {
        if (Mathf.Abs(Vector3.Distance(transform.position, upAnimatingPoint)) <= .5f)//if the airplane has reached the nextTargetPoint point
        {
            nextAnimatingPoint = downAnimatingPoint;
        }
        else if (Mathf.Abs(Vector3.Distance(transform.position, downAnimatingPoint)) <= .5f)
        {
            nextAnimatingPoint = upAnimatingPoint;
        }

        transform.position = Vector3.Lerp(transform.position, nextAnimatingPoint, animatingSpeed * Time.deltaTime / Vector3.Distance(transform.position, nextAnimatingPoint));//move the airplane to the nextAnimatingPoint
    }

    public void GoBackToBase()//let the airplane get back to the point which is above the airplane base to prepare to land
    {
        if (hadTakenOff)
        {
            isGoingBackToBase = true;
            nextTargetPoint = landingPoint;
        }
        else
        {
            hadTakenOff = true;
            hasToLand = true;
        }
    }

    private void Land()//let the airplane get down to the base
    {
        if (Mathf.Abs(transform.position.y - initialPosition.y) >= .5f)
        {
            transform.position += Vector3.down * takeOffSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, smoothRotatingSpeed / Quaternion.Angle(transform.rotation, initialRotation));
        }
        else
        {
            PlayLandSound();
            HasToDefend = false;
            isPatrolling = false;
            hasToLand = false;
            hadTakenOff = false;
            hasToPlayTakeOffSound = true;
            hasToPlayLandSound = true;
            for (int i = 0; i < smokeParticles.Length; i++)
            {
                smokeParticles[i].Stop();
            }
        }
    }

    public void OrderToShoot(Transform target)
    {
        this.target = target;
    }

    public void StopShooting()
    {
        target = null;

        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
        }

        IsShooting = false;
    }

    private IEnumerator Shoot(Transform target)//to creat projectiles and shoot them towards the target
    {
        while (IsShooting)
        {
            if (!hasToLand && hadTakenOff)
            {
                if (HasToUseRockets && isItTimeToShootAgain)
                {
                    StartCoroutine(WaitForShootingAgain());
                    if (securitySensor.Targets.Count > 0)
                    {
                        foreach (Creature trgt in securitySensor.Targets)
                        {
                            if (trgt != null)
                            {
                                for (int i = 0; i < airRockets.Count; i++)
                                {
                                    if (airRockets[i] != null && !airRockets[i].IsUsed)
                                    {
                                        airRockets[i].IsUsed = true;
                                        airRockets[i].transform.parent = null;
                                        //airRockets[i].transform.LookAt(trgt.transform);
                                        airRockets[i].FollowTarget(trgt);
                                        currentRocketsNumber--;
                                        UpdateAmmoStateText(true);
                                        goto next;
                                    }
                                }
                            }
                        next:;
                        }
                    }
                    yield return new WaitForSeconds(secondsBetweenEachRocket);
                }
                else if(!HasToUseRockets)
                {
                    if (currentBulletsNumber > 0)
                    {
                        audioSource.PlayOneShot(bulletSound.audioClip, bulletSound.volume);
                        currentBulletsNumber--;
                        UpdateAmmoStateText(false);
                        GameObject projectile = Instantiate(projectilePrefab, projectileCreatPoint.position, projectilePrefab.transform.rotation);
                        if (target != null)
                        {
                            projectile.transform.LookAt(target);
                            projectile.GetComponent<Projectile>().FollowTarget(target.GetComponent<Creature>());
                        }
                        yield return new WaitForSeconds(secondsBetweenEachProjectile);
                    }
                }
            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    private IEnumerator WaitForShootingAgain()
    {
        isItTimeToShootAgain = false;
        yield return new WaitForSeconds(secondsBetweenEachRocket);
        isItTimeToShootAgain = true;
    }

    private void ReloadWeapon(Constants.SuppliesTypes suppliesType, int ammoNumber)//to realod the waepon when the player takes ammo pack
    {
        switch (suppliesType)
        {
            case Constants.SuppliesTypes.RocketsAmmo:
                {
                    int counter = ammoNumber;

                    for (int i = 0; i < airRockets.Count; i++)//put the rockets in thiere positions in the weapon
                    {
                        if (counter > 0)
                        {
                            if (airRockets[i] == null || airRockets[i].IsUsed)
                            {
                                counter--;
                                currentRocketsNumber = Mathf.Clamp(++currentRocketsNumber, 0, maxRocketsNumber);
                                airRockets[i] = Instantiate(airplaneRocketPrefab, airRocketsReloadPoints[i].Parent).GetComponent<Projectile>();
                                airRockets[i].transform.localScale = Vector3.one;
                                airRockets[i].transform.localEulerAngles = Vector3.zero;
                                airRockets[i].transform.localPosition = airRocketsReloadPoints[i].InitialLocalPosition;
                            }
                        }
                    }
                    UpdateAmmoStateText(true);
                }
                break;

            case Constants.SuppliesTypes.BulletsAmmo:
                {
                    currentBulletsNumber = Mathf.Clamp(currentBulletsNumber + ammoNumber, 0, maxBulletsNumber);
                    UpdateAmmoStateText(false);
                }
                break;

            default: break;
        }
    }

    public void UpdateDefendingState(bool defendState)//to start or stop defending
    {
        if (defendState)
        {
            Defend();
        }
        else
        {
            GoBackToBase();
        }
    }

    public void SetWeaponToUse(bool hasToUseRockets)//to switch between rockets and bullets which the airplane use it to attack
    {
        if (hasToUseRockets)
        {
            HasToUseRockets = true;
        }
        else
        {
            HasToUseRockets = false;
        }
    }

    public void UpdateAmmoStateText(bool hasToUpdateRocketsNumber)//to change the ammo texts in the scene for thw weapon 
    {
        if (!hasToUpdateRocketsNumber)
        {
            bulletsAmmoStateText.text = currentBulletsNumber.ToString() + "/" + maxBulletsNumber.ToString();
        }
        else
        {
            rocketsAmmoStateText.text = currentRocketsNumber.ToString() + "/" + maxRocketsNumber.ToString();
        }
    }

    private void PlayTakeOffSound()
    {
        if (hasToPlayTakeOffSound)
        {
            hasToPlayTakeOffSound = false;
            hasToPlayLandSound = true;
            audioSource.Stop();
            audioSource.PlayOneShot(takeOffSound.audioClip, takeOffSound.volume);
        }
        
    }

    private void PlayLandSound()
    {
        if (hasToPlayLandSound)
        {
            hasToPlayLandSound = false;
            hasToPlayTakeOffSound = true;
            audioSource.Stop();
            audioSource.PlayOneShot(landSound.audioClip, landSound.volume);
        }
    }

    void OnDestroy()
    {
        EventsManager.onTakingAmmo -= ReloadWeapon;
        EventsManager.onLevelFinishs -= GoBackToBase;
        EventsManager.onLevelFinishs -= ShowUpdateImages;
        EventsManager.onLevelStarts -= HideUpdateImages;
    }
}