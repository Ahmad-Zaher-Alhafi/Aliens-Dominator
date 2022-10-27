using System;
using System.Collections;
using System.Collections.Generic;
using ManagersAndControllers;
using Projectiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Defence_Weapons {
    public class SecurityWeapon : MonoBehaviour {
        [HideInInspector]
        public bool IsShooting;
        [HideInInspector]
        public bool HasToDefend;
        public Constants.SecurityWeaponsTypes weaponType; //if it was air or ground defence
        [HideInInspector]
        public bool WasDestroyed;
        [SerializeField] private float smoothWeaponRotatingSpeed; //the speed of the rotation when the weapon is gonna rotate himself towards the target
        [SerializeField] private float smoothWeaponEscortingSpeed; //the speed of the rotation when the weapon is gonna rotate himself towards the random angle
        [SerializeField] private GameObject projectilePrefab; //bullet to threw
        [SerializeField] private Transform projectileCreatPoint; //bullet creat position
        [SerializeField] private float secondsBetweenEachProjectile;
        [SerializeField] private float maxEscortingAngleOnXAxis; //how much far could the new psotion could have on x Axis which the weapon is gonna rotate towards it
        [SerializeField] private float secondsBetweenEachEscort; //delay between each escort phase
        [SerializeField] private TextMeshProUGUI ammoStateText;
        [SerializeField] private int maxBulletsNumber;
        [SerializeField] private Transform ammoStateCanves;
        [SerializeField] private SecuritySensor securitySensor;
        [SerializeField] private GameObject weaponFixImg;
        [SerializeField] private GameObject updateWeaponFireRateImg;
        [SerializeField] private GameObject updateWeaponStrengthImg;
        [SerializeField] private int weaponLevel;
        [SerializeField] private float weaponHealth;
        [SerializeField] private Slider weaponHealthBar;
        [SerializeField] private float repairCost;
        //
        [Header("For ground weapon only")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Sound GroundGunStartShootingSound;
        [SerializeField] private Sound GroundGunStopShootingSound;
        [SerializeField] private Transform rotatingPart;
        //
        [Header("For anti air only")]
        [SerializeField] private List<Projectile> airRockets = new();
        [SerializeField] private GameObject airRocketPrefab;
        //

        private readonly List<AirRocketsReloadPoint> airRocketsReloadPoints = new();
        private int currentBulletsNumber;
        private Vector3 escortPoint; //point where the weapon is gonna look towards
        private GameHandler gameHandler;
        private bool hasToLookAtTheTarget; //if there is a target to shoot at
        private bool hasToPlayStartShootingSound;
        private bool hasToPlayStopShootingSound;
        private bool hasToResetRotation; //if the weapon should reset it's position after finishing shooting at targets
        private float initialHealth;
        private bool isCoolingDown;
        private bool isEscorting; //true if the waepon rotating around towards a random points (just to make cool effects that the weapon is escorting the area)
        private Quaternion originalEuralAngles; //the rotation which are we gonna use them to reset the weapon rotation
        private Transform playerPointToLookAt;
        private Coroutine shootCoroutine;
        private Transform target;

        private void Start() {
            gameHandler = FindObjectOfType<GameHandler>();
            isCoolingDown = false;
            initialHealth = weaponHealth;
            weaponHealthBar.maxValue = initialHealth;
            weaponHealthBar.minValue = 0;
            weaponHealthBar.normalizedValue = weaponHealth / initialHealth;
            EventsManager.onLevelFinishs += ShowUpdateImages;
            EventsManager.onLevelStarts += HideUpdateImages;

            WasDestroyed = false;
            hasToPlayStartShootingSound = true;
            hasToPlayStopShootingSound = true;
            playerPointToLookAt = GameObject.FindGameObjectWithTag(Constants.PlayerAttackPoint).transform;
            currentBulletsNumber = maxBulletsNumber;
            UpdateAmmoStateText();

            if (weaponType == Constants.SecurityWeaponsTypes.air)
                for (int i = 0; i < airRockets.Count; i++)
                    airRocketsReloadPoints.Add(new AirRocketsReloadPoint(airRockets[i].transform.parent.transform, airRockets[i].transform.localPosition));

            EventsManager.onTakingAmmo += ReloadWeapon;

            hasToResetRotation = false;
            originalEuralAngles = transform.rotation;
            isEscorting = false;
            IsShooting = false;
            hasToLookAtTheTarget = false;

            StartCoroutine(EscortArea());
        }

        private void Update() {
            ammoStateCanves.LookAt(playerPointToLookAt);

            if (!HasToDefend) return;

            if (weaponType == Constants.SecurityWeaponsTypes.ground && currentBulletsNumber > 0) {
                if (!IsShooting) rotatingPart.Rotate(Vector3.right * 200 * Time.deltaTime);
                else rotatingPart.Rotate(Vector3.right * 500 * Time.deltaTime);
            }

            if (isEscorting) LookTowardsEscortPoint(escortPoint);
            else if (hasToResetRotation) ResetRotation();
        }

        private void LateUpdate() {
            if (hasToLookAtTheTarget) LookAtTheTarget(target);
        }

        private void OnDestroy() {
            if (weaponType == Constants.SecurityWeaponsTypes.air) EventsManager.onTakingAmmo -= ReloadWeapon;

            EventsManager.onLevelFinishs -= ShowUpdateImages;
        }

        private void ShowUpdateImages() {
            if (weaponHealth != initialHealth || currentBulletsNumber != maxBulletsNumber) weaponFixImg.SetActive(true);

            updateWeaponFireRateImg.SetActive(true);
            updateWeaponStrengthImg.SetActive(true);
        }

        private void HideUpdateImages() {
            weaponFixImg.SetActive(false);
            updateWeaponFireRateImg.SetActive(false);
            updateWeaponStrengthImg.SetActive(false);
        }

        public void OrderToShoot(Transform target) {
            if (currentBulletsNumber > 0) {
                isEscorting = false;
                this.target = target;
                hasToLookAtTheTarget = true;
            } else {
                StopShooting();

                if (!hasToResetRotation) hasToResetRotation = true;
            }
        }

        public void StopShooting() {
            target = null;
            if (shootCoroutine != null) StopCoroutine(shootCoroutine);
            IsShooting = false;
            hasToLookAtTheTarget = false;
            if (weaponType == Constants.SecurityWeaponsTypes.ground) PlayStopShootingSound();

            if (currentBulletsNumber > 0) isEscorting = true;
        }

        private void LookAtTheTarget(Transform target) //to make the weapon look toward the target
        {
            Quaternion oldQuat = transform.rotation;
            transform.LookAt(target);
            Quaternion targetAngle = transform.rotation;
            transform.rotation = oldQuat;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, smoothWeaponRotatingSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetAngle) <= 10) //if the weapon finished rotating to the correct angle wihch is the target angle
            {
                IsShooting = true;

                if (!isCoolingDown) {
                    if (shootCoroutine != null) StopCoroutine(shootCoroutine);
                    shootCoroutine = StartCoroutine(Shoot(target)); //give orders to weapon to rotate towards the target to shootCoroutine
                }
            }
        }

        private IEnumerator SetCoolDownTime() {
            if (!isCoolingDown) {
                isCoolingDown = true;
                yield return new WaitForSeconds(secondsBetweenEachProjectile);
                isCoolingDown = false;
            }
        }

        private void LookTowardsEscortPoint(Vector3 escortPoint) //to let the weapon look towards random points(escorting area)
        {

            Quaternion oldQuat = transform.rotation;
            transform.LookAt(escortPoint);
            Quaternion targetAngle = transform.rotation;
            transform.rotation = oldQuat;

            if (Quaternion.Angle(transform.rotation, targetAngle) > .2f) transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, smoothWeaponEscortingSpeed * Time.deltaTime);
            else GenerateEscortPoint();
        }

        private IEnumerator Shoot(Transform target) //to creat projectiles and shoot them towards the target
        {
            while (IsShooting) {
                if (target == null || currentBulletsNumber <= 0) break;

                if (weaponType == Constants.SecurityWeaponsTypes.air) {
                    foreach (Projectile rocket in airRockets)
                        if (rocket != null && !rocket.IsUsed && !isCoolingDown) {
                            currentBulletsNumber--;
                            UpdateAmmoStateText();
                            rocket.DamageCost *= weaponLevel;
                            rocket.IsUsed = true;
                            rocket.transform.parent = null;
                            if (target != null) {
                                rocket.transform.LookAt(target);
                                rocket.FollowTarget(target.GetComponent<Creatures.Creature>());
                            }
                            StartCoroutine(SetCoolDownTime());
                        }
                } else if (weaponType == Constants.SecurityWeaponsTypes.ground) {
                    if (currentBulletsNumber > 0 && !isCoolingDown) {
                        PlayStartShootingSound();
                        currentBulletsNumber--;
                        UpdateAmmoStateText();
                        GameObject projectile = Instantiate(projectilePrefab, projectileCreatPoint.position, projectilePrefab.transform.rotation);
                        projectile.transform.LookAt(target);
                        var projectileScript = projectile.GetComponent<Projectile>();
                        projectileScript.DamageCost *= weaponLevel;
                        projectileScript.FollowTarget(target.GetComponent<Creatures.Creature>());
                        StartCoroutine(SetCoolDownTime());
                    }
                }
                yield return null;
            }
        }

        private IEnumerator EscortArea() //to control the escorting process
        {
            while (!IsShooting) {
                if (isEscorting) {
                    yield return new WaitForSeconds(secondsBetweenEachEscort);
                } else {
                    if (!hasToLookAtTheTarget && !hasToResetRotation) //if is not shooting and not escorting and not resetting it's rotation
                    {
                        Vector3 rotation = transform.rotation.eulerAngles;

                        if (Mathf.Abs(rotation.x) >= 1 || Mathf.Abs(rotation.z) >= 1) //if it's roation needs to be reseted
                        {
                            hasToResetRotation = true;
                        } else //if it has to escort
                        {
                            GenerateEscortPoint();
                            isEscorting = true;
                        }

                        yield return new WaitForSeconds(secondsBetweenEachEscort);
                        isEscorting = false;
                    }
                }

                yield return new WaitForSeconds(0.001f);
            }
        }

        private void GenerateEscortPoint() {
            escortPoint = new Vector3(Random.Range(-maxEscortingAngleOnXAxis, maxEscortingAngleOnXAxis), transform.position.y, transform.position.z - 10);
        }

        private void ResetRotation() //to reset the weapon rotation after finishing shooting at target
        {
            if (Quaternion.Angle(transform.rotation, originalEuralAngles) >= .2f) transform.rotation = Quaternion.Slerp(transform.rotation, originalEuralAngles, smoothWeaponEscortingSpeed * Time.deltaTime);
            else hasToResetRotation = false;
        }

        private void ReloadWeapon(Constants.SuppliesTypes suppliesType, int ammoNumber) //to realod the waepon when the player takes ammo pack
        {
            switch (suppliesType) {
                case Constants.SuppliesTypes.RocketsAmmo: {
                    if (weaponType == Constants.SecurityWeaponsTypes.air) {
                        int counter = ammoNumber;

                        for (int i = 0; i < airRockets.Count; i++) //put the rockets in thiere positions in the weapon
                            if (counter > 0)
                                if (airRockets[i] == null || airRockets[i].IsUsed) {
                                    counter--;
                                    currentBulletsNumber = Mathf.Clamp(++currentBulletsNumber, 0, maxBulletsNumber);
                                    airRockets[i] = Instantiate(airRocketPrefab, airRocketsReloadPoints[i].Parent).GetComponent<Projectile>();
                                    airRockets[i].transform.localScale = Vector3.one;
                                    airRockets[i].transform.localEulerAngles = Vector3.zero;
                                    airRockets[i].transform.localPosition = airRocketsReloadPoints[i].InitialLocalPosition;
                                }
                        UpdateAmmoStateText();
                    }
                }
                    break;

                case Constants.SuppliesTypes.BulletsAmmo: {
                    if (weaponType == Constants.SecurityWeaponsTypes.ground) {
                        currentBulletsNumber = Mathf.Clamp(currentBulletsNumber + ammoNumber, 0, maxBulletsNumber);
                        UpdateAmmoStateText();
                    }
                }
                    break;

            }
        }

        public void UpdateDefendingState(bool defendState) //to start or stop defending
        {
            HasToDefend = defendState;

            if (!defendState) StopShooting();
        }

        public void UpdateAmmoStateText() {
            ammoStateText.text = currentBulletsNumber + "/" + maxBulletsNumber;
        }

        private void PlayStartShootingSound() {
            if (hasToPlayStartShootingSound) {
                audioSource.Stop();
                hasToPlayStartShootingSound = false;
                hasToPlayStopShootingSound = true;
                audioSource.clip = GroundGunStartShootingSound.audioClip;
                audioSource.volume = GroundGunStartShootingSound.volume;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void PlayStopShootingSound() {
            if (hasToPlayStopShootingSound) {
                audioSource.Stop();
                hasToPlayStopShootingSound = false;
                hasToPlayStartShootingSound = true;
                audioSource.clip = GroundGunStopShootingSound.audioClip;
                audioSource.volume = GroundGunStopShootingSound.volume;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        public void UpdateWeaponFireRate(float secondsToDecrease) {
            secondsBetweenEachProjectile -= secondsToDecrease;
            print("WeaponBoosted " + secondsBetweenEachProjectile);
        }

        public void UpdateWeaponStrength() {
            weaponLevel++;
            print("LeveledUp " + weaponLevel);
        }

        public void FixWeapon() {
            if (gameHandler.NumOfResources >= repairCost) {
                if (weaponHealth <= 0) {
                    weaponHealth = initialHealth;
                    gameObject.SetActive(true);
                    ResetRotation();
                    UpdateDefendingState(false);
                    WasDestroyed = false;
                } else {
                    weaponHealth += 25;
                }

                gameHandler.UpdateResourcesCount(repairCost);
                weaponHealthBar.normalizedValue = weaponHealth / initialHealth;
                print("WeaponFixed " + weaponHealth);
            } else {
                print("Not Enough Resources To Repair");
            }
        }

        public void TakeDamage(float damageCost) {
            weaponHealth -= damageCost;
            weaponHealthBar.normalizedValue = weaponHealth / initialHealth;

            if (weaponHealth <= 0) {
                weaponHealthBar.normalizedValue = 0;
                weaponHealth = 0;
                WasDestroyed = true;
                gameObject.SetActive(false);
                EventsManager.OnSecurityWeaponDestroy();
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