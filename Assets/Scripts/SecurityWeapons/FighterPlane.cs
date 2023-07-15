using System;
using System.Collections;
using System.Collections.Generic;
using Creatures;
using FiniteStateMachine;
using FiniteStateMachine.FighterPlaneStateMachine;
using Projectiles;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SecurityWeapons {
    public class FighterPlane : MonoBehaviour, IAutomatable {
        public GameObject GameObject => gameObject;
        public bool IsDestroyed => false;

        public bool HasToTakeOff { get; private set; }
        public bool IsShooting { get; private set; }
        public bool HasToUseRockets { get; private set; } //true then use rockets, false then use bullets

        [SerializeField] private Transform takeOffPoint;
        /// <summary>
        /// Point where the plane will go to while taking off
        /// </summary>
        public Transform TakeOffPoint => takeOffPoint;
        public float TakeOffSpeed => takeOffSpeed;
        [SerializeField] private float takeOffSpeed;

        [SerializeField] private List<Transform> airPathPoint = new(); //air points which the creature has to follow
        [SerializeField] private float patrolSpeed; //speed of moveming between the airplane wayoints
        [SerializeField] private float animatingSpeed; //speed of going up and down in animating phase
        [SerializeField] private float smoothRotatingSpeed; //speed of rotating towards a point
        [SerializeField] private Transform pointToLookAt; //point to look at if there was no target to look at
        [SerializeField] private float secondsBetwennPatrols; //secnods to wait before going to next pathPoint
        [SerializeField] private float heightOnAnimation; //how much hight should the airplane should move aup and down on the y axis
        [SerializeField] private Transform landingPoint; //the point where the airplane should back to it before it lands(this point is above the airplane base)
        [SerializeField] private GameObject airplaneRocketPrefab;
        [SerializeField] private List<Projectile> airRockets = new(); //rockets of the airplane
        [SerializeField] private int maxBulletsNumber;
        [SerializeField] private int maxRocketsNumber;
        [SerializeField] private Transform ammoStateCanves;
        [SerializeField] private GameObject projectilePrefab; //bullet to threw
        [SerializeField] private Transform projectileCreatPoint; //bullet creat position
        [SerializeField] private float secondsBetweenEachProjectile;
        [SerializeField] private float secondsBetweenEachRocket;
        [FormerlySerializedAs("securitySensor")]
        [SerializeField] private WeaponSensor<Creature> weaponSensor;
        [SerializeField] private float aimAccuracy; //the bigger the number the less the rotating accuracy which let the airplane shoot even before reaching the exact target angle(becasue the airplane shoot at the target after it rotates towads the target)
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
        private int airPointIndex; //index of the next targetWayPoint in the airPathPoint array
        private readonly List<AirRocketsReloadPoint> airRocketsReloadPoints = new();
        private int currentBulletsNumber;
        private int currentRocketsNumber;
        private Vector3 downAnimatingPoint; //dwon animating point where the airplane is gonna use it to animate dwon
        private bool hadTakenOff;
        private bool hasToAnimate;
        private bool hasToLand;
        private bool hasToPlayLandSound;
        private bool hasToPlayTakeOffSound;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool isGoingBackToBase;
        private bool isItTimeToShootAgain; //true if the airplane waited for a time before shooting towardes all the creatures again(needed only when using rockets to prevent shooting all the rockets at the same creatures)
        private bool isPatrolling;
        private Vector3 nextAnimatingPoint; //to switch between (upAnimatingPoint && downAnimatingPoint)

        private Transform nextTargetPoint; //nextTargetPoint is the next point that the airplane is going to
        private Transform playerPointToLookAt;
        private Coroutine shootCoroutine;
        private IDamageable target;
        private Vector3 upAnimatingPoint; //up animating point where the airplane is gonna use it to animate up
        private FighterPlaneStateMachine fighterPlaneStateMachine;

        private void Awake() {
            fighterPlaneStateMachine = GetComponent<FighterPlaneStateMachine>();
            fighterPlaneStateMachine.Init(this, FighterPlaneStateType.Deactivated);
        }

        private void ShowUpdateImages() {
            weaponFixImg.SetActive(true);
            updateWeaponFireRateImg.SetActive(true);
            updateWeaponStrengthImg.SetActive(true);
        }

        private void HideUpdateImages() {
            weaponFixImg.SetActive(false);
            updateWeaponFireRateImg.SetActive(false);
            updateWeaponStrengthImg.SetActive(false);
        }

        public void Defend() {
            PlayTakeOffSound();
            HasToTakeOff = true;

            if (hasToLand) {
                hasToLand = false;
                hadTakenOff = false;
            }

            isGoingBackToBase = false;

            airPointIndex = 0;
            nextTargetPoint = airPathPoint[0];
            for (int i = 0; i < smokeParticles.Length; i++) {
                smokeParticles[i].Play();
            }
        }

        public IEnumerator PatrollAround() {
            while (HasToTakeOff && !hasToLand && hadTakenOff)
                if (Mathf.Abs(Vector3.Distance(transform.position, nextTargetPoint.position)) <= .5f) //if the airplane has reached the nextTargetPoint point
                {
                    if (isGoingBackToBase) {
                        isGoingBackToBase = false;
                        hasToLand = true;
                        break;
                    }
                    if (airPointIndex + 1 < airPathPoint.Count) //if the next index is not out of range of the array
                        airPointIndex++; //get the next point index
                    else airPointIndex = 0;

                    nextTargetPoint = airPathPoint[airPointIndex];

                    upAnimatingPoint = transform.position + Vector3.up * heightOnAnimation; //set the upAnimatingPoint
                    downAnimatingPoint = transform.position + Vector3.down * heightOnAnimation; //set the downAnimatingPoint
                    nextAnimatingPoint = upAnimatingPoint; //let the animating starts with upAnimatingPoint

                    hasToAnimate = true; //order to animate untill the patroll seconds wait finish
                    yield return new WaitForSeconds(secondsBetwennPatrols);
                    hasToAnimate = false; //to stop animating and keep moving to next pathPoint
                } else {
                    transform.position = Vector3.Lerp(transform.position, nextTargetPoint.position, patrolSpeed * Time.deltaTime / Vector3.Distance(transform.position, nextTargetPoint.position)); //move the airplane to the nextTargetPoint
                    yield return new WaitForSeconds(.001f);
                }
            isPatrolling = false;
        }

        /// <summary>
        ///     To let the airplane look towards the objectToLookAt
        /// </summary>
        /// <param name="objectToLookAt">the object that you want the airplane to look at while he is moving</param>
        /// <param name="isItTarget">
        ///     true if it is rotating towards a target (then it has to shoot after finishing rotating) else
        ///     it's rotating towards some point(so it does not have to shoot after rotating)
        /// </param>
        private void RotateToTheWantedAngle(Transform objectToLookAt, bool isItTarget) {
            Quaternion wantedAngle = transform.rotation;

            if (HasToUseRockets && !isItTarget || !HasToUseRockets) //if it was using rockets and there were no target or if it was not using rockets(because using rockets does not need to look at the player because the air plane is gonna shoot all the target at once using multi rockets so no need to look at the targets)
            {
                //I'm doing that as a trick to get the wanted angle and after that i'm resetting the angle to it's old angle and that because we need to rotates the airplane smoothly and not suddenly which make it cooler
                Quaternion oldAngle = transform.rotation; //save old angle
                transform.LookAt(objectToLookAt); //look at the target
                wantedAngle = transform.rotation; //get the wanted eural angle after he looked
                transform.rotation = oldAngle; //reset the angle to the old angle 
                transform.rotation = Quaternion.Lerp(transform.rotation, wantedAngle, smoothRotatingSpeed / Quaternion.Angle(transform.rotation, wantedAngle)); //rotate the airplane smoothly from old angle to the new one
            }

            if (isItTarget) {
                if (HasToUseRockets && !IsShooting) {
                    IsShooting = true;
                    shootCoroutine = StartCoroutine(Shoot(target)); //give orders to airplane to rotate towards the target to shootCoroutine
                } else if (!HasToUseRockets && !IsShooting && Quaternion.Angle(transform.rotation, wantedAngle) <= aimAccuracy) //if the airplane finished rotating to the correct angle wihch is the target angle
                {
                    IsShooting = true;
                    shootCoroutine = StartCoroutine(Shoot(target)); //give orders to airplane to rotate towards the target to shootCoroutine
                }
            }
        }

        private void TakeOff() {
            if (Mathf.Abs(transform.position.y - nextTargetPoint.position.y) >= .5f) transform.position += Vector3.up * takeOffSpeed * Time.deltaTime; //make the airplane moves upwards
            else hadTakenOff = true;
        }

        private void Animate() {
            //to let the airplane move up and down in small movement
            if (Mathf.Abs(Vector3.Distance(transform.position, upAnimatingPoint)) <= .5f) //if the airplane has reached the nextTargetPoint point
                nextAnimatingPoint = downAnimatingPoint;
            else if (Mathf.Abs(Vector3.Distance(transform.position, downAnimatingPoint)) <= .5f) nextAnimatingPoint = upAnimatingPoint;

            transform.position = Vector3.Lerp(transform.position, nextAnimatingPoint, animatingSpeed * Time.deltaTime / Vector3.Distance(transform.position, nextAnimatingPoint)); //move the airplane to the nextAnimatingPoint
        }

        public void GoBackToBase() {
            //let the airplane get back to the point which is above the airplane base to prepare to land
            if (hadTakenOff) {
                isGoingBackToBase = true;
                nextTargetPoint = landingPoint;
            } else {
                hadTakenOff = true;
                hasToLand = true;
            }
        }

        private void Land() {
            //let the airplane get down to the base
            if (Mathf.Abs(transform.position.y - initialPosition.y) >= .5f) {
                transform.position += Vector3.down * takeOffSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, smoothRotatingSpeed / Quaternion.Angle(transform.rotation, initialRotation));
            } else {
                PlayLandSound();
                HasToTakeOff = false;
                isPatrolling = false;
                hasToLand = false;
                hadTakenOff = false;
                hasToPlayTakeOffSound = true;
                hasToPlayLandSound = true;
                for (int i = 0; i < smokeParticles.Length; i++) smokeParticles[i].Stop();
            }
        }

        public void OrderToShoot(IDamageable target) {
            this.target = target;
        }

        public void StopShooting() {
            target = null;

            if (shootCoroutine != null) StopCoroutine(shootCoroutine);

            IsShooting = false;
        }

        private IEnumerator Shoot(IDamageable target) {
            //to creat projectiles and shoot them towards the target
            while (IsShooting) {
                if (!hasToLand && hadTakenOff) {
                    if (HasToUseRockets && isItTimeToShootAgain) {
                        StartCoroutine(WaitForShootingAgain());
                        if (weaponSensor.Targets.Count > 0)
                            foreach (Creatures.Creature trgt in weaponSensor.Targets) {
                                if (trgt != null)
                                    for (int i = 0; i < airRockets.Count; i++)
                                        if (airRockets[i] != null && !airRockets[i].IsUsed) {
                                            airRockets[i].IsUsed = true;
                                            airRockets[i].transform.parent = null;
                                            //airRockets[i].transform.LookAt(trgt.transform);
                                            airRockets[i].FollowTarget(target);
                                            currentRocketsNumber--;
                                            UpdateAmmoStateText(true);
                                            goto next;
                                        }
                                next: ;
                            }
                        yield return new WaitForSeconds(secondsBetweenEachRocket);
                    } else if (!HasToUseRockets) {
                        if (currentBulletsNumber > 0) {
                            audioSource.PlayOneShot(bulletSound.audioClip, bulletSound.volume);
                            currentBulletsNumber--;
                            UpdateAmmoStateText(false);
                            GameObject projectile = Instantiate(projectilePrefab, projectileCreatPoint.position, projectilePrefab.transform.rotation);
                            if (target != null) {
                                projectile.transform.LookAt(target.GameObject.transform);
                                projectile.GetComponent<Projectile>().FollowTarget(target);
                            }
                            yield return new WaitForSeconds(secondsBetweenEachProjectile);
                        }
                    }
                }
                yield return new WaitForSeconds(0.001f);
            }
        }

        private IEnumerator WaitForShootingAgain() {
            isItTimeToShootAgain = false;
            yield return new WaitForSeconds(secondsBetweenEachRocket);
            isItTimeToShootAgain = true;
        }

        private void ReloadWeapon(Constants.SuppliesTypes suppliesType, int ammoNumber) {
            //to realod the waepon when the player takes ammo pack
            switch (suppliesType) {
                case Constants.SuppliesTypes.RocketsAmmo: {
                    int counter = ammoNumber;

                    for (int i = 0; i < airRockets.Count; i++) //put the rockets in thiere positions in the weapon
                        if (counter > 0)
                            if (airRockets[i] == null || airRockets[i].IsUsed) {
                                counter--;
                                currentRocketsNumber = Mathf.Clamp(++currentRocketsNumber, 0, maxRocketsNumber);
                                airRockets[i] = Instantiate(airplaneRocketPrefab, airRocketsReloadPoints[i].Parent).GetComponent<Projectile>();
                                airRockets[i].transform.localScale = Vector3.one;
                                airRockets[i].transform.localEulerAngles = Vector3.zero;
                                airRockets[i].transform.localPosition = airRocketsReloadPoints[i].InitialLocalPosition;
                            }
                    UpdateAmmoStateText(true);
                }
                    break;

                case Constants.SuppliesTypes.BulletsAmmo: {
                    currentBulletsNumber = Mathf.Clamp(currentBulletsNumber + ammoNumber, 0, maxBulletsNumber);
                    UpdateAmmoStateText(false);
                }
                    break;

            }
        }

        public void UpdateDefendingState(bool defendState) {
            //to start or stop defending
            if (defendState) Defend();
            else GoBackToBase();
        }

        public void SetWeaponToUse(bool hasToUseRockets) {
            //to switch between rockets and bullets which the airplane use it to attack
            if (hasToUseRockets) HasToUseRockets = true;
            else HasToUseRockets = false;
        }

        public void UpdateAmmoStateText(bool hasToUpdateRocketsNumber) {
            //to change the ammo texts in the scene for thw weapon 
            if (!hasToUpdateRocketsNumber) bulletsAmmoStateText.text = currentBulletsNumber + "/" + maxBulletsNumber;
            else rocketsAmmoStateText.text = currentRocketsNumber + "/" + maxRocketsNumber;
        }

        private void PlayTakeOffSound() {
            if (hasToPlayTakeOffSound) {
                hasToPlayTakeOffSound = false;
                hasToPlayLandSound = true;
                audioSource.Stop();
                audioSource.PlayOneShot(takeOffSound.audioClip, takeOffSound.volume);
            }

        }

        private void PlayLandSound() {
            if (hasToPlayLandSound) {
                hasToPlayLandSound = false;
                hasToPlayTakeOffSound = true;
                audioSource.Stop();
                audioSource.PlayOneShot(landSound.audioClip, landSound.volume);
            }
        }

        [Serializable]
        private class Sound {
            public AudioClip audioClip;
            public float volume;
        }

        private class AirRocketsReloadPoint {
            public readonly Vector3 InitialLocalPosition;
            public readonly Transform Parent;

            public AirRocketsReloadPoint(Transform parent, Vector3 initialLocalPosition) {
                Parent = parent;
                InitialLocalPosition = initialLocalPosition;
            }
        }
    }
}