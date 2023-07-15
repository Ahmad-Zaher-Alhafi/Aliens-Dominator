using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Creatures;
using FiniteStateMachine;
using FiniteStateMachine.FighterPlaneStateMachine;
using Projectiles;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace SecurityWeapons {
    public class FighterPlane : MonoBehaviour, IAutomatable, IWeaponSpecification {
        public FighterPlaneStateType CurrentStateType => fighterPlaneStateMachine.PrimaryState.Type;
        public GameObject GameObject => gameObject;
        public bool IsDestroyed => false;

        public bool HasToTakeOff { get; set; }
        public bool HasToGoBack { get; private set; }
        public bool IsShooting { get; private set; }

        [SerializeField]
        private bool hasToUseRockets;
        public bool HasToUseRockets => hasToUseRockets;

        [Header("Define the random target position that weapon will look at while guarding")]
        [SerializeField] private Vector2 guardingXRange;
        [SerializeField] private Vector2 guardingYRange;
        public Vector3 RotateXRange => guardingXRange;
        public Vector3 RotateYRange => guardingYRange;

        [SerializeField] private Transform takeOffPoint;
        /// <summary>
        /// Point where the plane will go to while taking off
        /// </summary>
        public Transform TakeOffPoint => takeOffPoint;
        [SerializeField] private Transform landingPoint;
        /// <summary>
        /// The point where the plane should go back to before it lands
        /// </summary>
        public Transform LandingPoint => landingPoint;
        public float TakeOffSpeed => takeOffSpeed;
        [SerializeField] private float takeOffSpeed;

        [SerializeField] private List<Transform> patrollingPoints = new();
        public IReadOnlyList<Transform> PatrollingPoints => patrollingPoints;

        [SerializeField] private float patrollingSpeed;
        public float PatrollingSpeed => patrollingSpeed;

        [SerializeField] private float rotateSpeed = 2;
        public float RotateSpeed => rotateSpeed;

        [SerializeField] private float aimingSpeed = 15;
        public float AimingSpeed => aimingSpeed;

        [SerializeField] private float bulletsPerSecond = 4;
        public float BulletsPerSecond => bulletsPerSecond;

        [SerializeField] private WeaponSensor<Creature> weaponSensor;
        public WeaponSensor<Creature> WeaponSensor => weaponSensor;
        public bool HasLanded { get; set; }

        [SerializeField] private float animatingSpeed = 1; 
        public float AnimatingSpeed => animatingSpeed;
        
        private readonly Dictionary<RocketsReloadPoint, Projectile> rockets = new();
        
        
        [SerializeField] private float smoothRotatingSpeed; //speed of rotating towards a point
        [SerializeField] private Transform pointToLookAt; //point to look at if there was no target to look at
        [SerializeField] private float secondsBetwennPatrols; //secnods to wait before going to next pathPoint
        [SerializeField] private float heightOnAnimation; //how much hight should the airplane should move aup and down on the y axis
        [SerializeField] private GameObject airplaneRocketPrefab;
        [SerializeField] private List<Projectile> airRockets = new(); //rockets of the airplane
        [SerializeField] private int maxBulletsNumber;
        [SerializeField] private int maxRocketsNumber;
        [SerializeField] private Transform ammoStateCanves;
        [SerializeField] private GameObject projectilePrefab; //bullet to threw
        [FormerlySerializedAs("projectileCreatPoint")]
        [SerializeField] private Transform projectileCreatePoint; //bullet creat position
        [SerializeField] private float secondsBetweenEachProjectile;
        [SerializeField] private float secondsBetweenEachRocket;
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
            foreach (Projectile projectile in GetComponentsInChildren<Projectile>()) {
                rockets.Add(new RocketsReloadPoint(projectile.transform.parent, projectile.transform.position), projectile);
            }

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

        public void Activate() {
            if (IsDestroyed) return;

            PlayTakeOffSound();
            foreach (var particles in smokeParticles) {
                particles.Play();
            }
        }

        public void Deactivate() {
            if (IsDestroyed) return;

            PlayLandSound();
            foreach (var particles in smokeParticles) {
                particles.Stop();
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
                    if (airPointIndex + 1 < patrollingPoints.Count) //if the next index is not out of range of the array
                        airPointIndex++; //get the next point index
                    else airPointIndex = 0;

                    nextTargetPoint = patrollingPoints[airPointIndex];

                    upAnimatingPoint = transform.position + Vector3.up * heightOnAnimation; //set the upAnimatingPoint
                    downAnimatingPoint = transform.position + Vector3.down * heightOnAnimation; //set the downAnimatingPoint
                    nextAnimatingPoint = upAnimatingPoint; //let the animating starts with upAnimatingPoint

                    hasToAnimate = true; //order to animate untill the patroll seconds wait finish
                    yield return new WaitForSeconds(secondsBetwennPatrols);
                    hasToAnimate = false; //to stop animating and keep moving to next pathPoint
                } else {
                    transform.position = Vector3.Lerp(transform.position, nextTargetPoint.position, patrollingSpeed * Time.deltaTime / Vector3.Distance(transform.position, nextTargetPoint.position)); //move the airplane to the nextTargetPoint
                    yield return new WaitForSeconds(.001f);
                }
            isPatrolling = false;
        }

        private void TakeOff() {
            if (Mathf.Abs(transform.position.y - nextTargetPoint.position.y) >= .5f) transform.position += Vector3.up * takeOffSpeed * Time.deltaTime; //make the airplane moves upwards
            else hadTakenOff = true;
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

        public void Shoot(IDamageable target) {
            if (HasToUseRockets) {
                KeyValuePair<RocketsReloadPoint, Projectile> projectile = rockets.FirstOrDefault(pair => !pair.Key.isUed);
                if (projectile.Value == null) return;

                projectile.Key.isUed = true;
                projectile.Value.FollowTarget(target);

            } else {
                GameObject projectile = Instantiate(projectilePrefab, projectileCreatePoint.position, projectilePrefab.transform.rotation);
                projectile.GetComponent<Rigidbody>().AddRelativeForce(transform.forward * 200, ForceMode.Impulse);
            }
        }

        public void Reload(int ammoNumber) {
            foreach (RocketsReloadPoint rocketsReloadPoint in rockets.Keys) {
                if (!rocketsReloadPoint.isUed) return;
                if (ammoNumber <= 0) return;

                ammoNumber--;

                rocketsReloadPoint.isUed = false;

                rockets[rocketsReloadPoint] = Instantiate(projectilePrefab, rocketsReloadPoint.Parent).GetComponent<Projectile>();
                rockets[rocketsReloadPoint].transform.localScale = Vector3.one;
                rockets[rocketsReloadPoint].transform.localEulerAngles = Vector3.zero;
                rockets[rocketsReloadPoint].transform.position = rocketsReloadPoint.InitialPosition;
            }

            //UpdateAmmoStateText();
        }

        /*private IEnumerator Shoot(IDamageable target) {
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
        }*/

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
            if (defendState) Activate();
            else GoBackToBase();
        }

        public void SetWeaponToUse(bool hasToUseRockets) {
            //to switch between rockets and bullets which the airplane use it to attack
            if (hasToUseRockets) this.hasToUseRockets = true;
            else hasToUseRockets = false;
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

        private class RocketsReloadPoint {
            public bool isUed;
            public readonly Vector3 InitialPosition;
            public readonly Transform Parent;

            public RocketsReloadPoint(Transform parent, Vector3 initialPosition) {
                Parent = parent;
                InitialPosition = initialPosition;
            }
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(FighterPlane))]
        public class FighterPlaneEditor : Editor {
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                FighterPlane fighterPlane = (FighterPlane) target;

                // Activate the plane button
                if (GUILayout.Button("Activate plane")) {
                    if (Application.isPlaying) {
                        fighterPlane.HasToTakeOff = true;
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}