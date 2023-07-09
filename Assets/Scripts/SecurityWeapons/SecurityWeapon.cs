using System;
using System.Collections.Generic;
using Context;
using FiniteStateMachine;
using FiniteStateMachine.SecurityWeaponMachine;
using ManagersAndControllers;
using Projectiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SecurityWeapons {
    public abstract class SecurityWeapon : MonoBehaviour, IAutomatable {
        [HideInInspector]
        public bool IsShooting;
        [HideInInspector]
        public bool HasToDefend;
        public GameObject GameObject => gameObject;
        public Enum CurrentStateType => securityWeaponStateMachine.PrimaryState.Type;
        public bool IsDestroyed => weaponHealth <= 0;
        [SerializeField] private float guardingSpeed;
        public float GuardingSpeed => guardingSpeed;

        [SerializeField] private float aimingSpeed;
        public float AimingSpeed => aimingSpeed;

        [SerializeField] private float bulletsPerSecond;
        public float BulletsPerSecond => bulletsPerSecond;
        [SerializeField] protected GameObject projectilePrefab; //bullet to threw
        [SerializeField] protected Transform projectileCreatePoint; //bullet creat position

        [Header("Define the random target position that weapon will look at while guarding")]
        [SerializeField] private Vector2 guardingXRange;
        [SerializeField] private Vector2 guardingYRange;
        public Vector2 GuardingXRange => guardingXRange;
        public Vector2 GuardingYRange => guardingYRange;

        [SerializeField] private TextMeshProUGUI ammoStateText;
        [SerializeField] protected int maxBulletsNumber;
        [SerializeField] private Transform ammoStateCanves;

        [SerializeField] private WeaponSensor weaponSensor;
        public WeaponSensor WeaponSensor => weaponSensor;

        [SerializeField] private GameObject weaponFixImg;
        [SerializeField] private GameObject updateWeaponFireRateImg;
        [SerializeField] private GameObject updateWeaponStrengthImg;
        [SerializeField] private int weaponLevel;
        [SerializeField] private float weaponHealth = 200;
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


        protected int currentBulletsNumber;
        private Vector3 escortPoint; //point where the weapon is gonna look towards
        private GameController gameController;
        private bool hasToLookAtTheTarget; //if there is a target to shoot at
        private bool hasToPlayStartShootingSound;
        private bool hasToPlayStopShootingSound;
        private bool hasToResetRotation; //if the weapon should reset it's position after finishing shooting at targets
        private float initialHealth;
        private bool isCoolingDown;
        private bool isEscorting; //true if the waepon rotating around towards a random points (just to make cool effects that the weapon is escorting the area)
        private Quaternion originalEuralAngles; //the rotation which are we gonna use them to reset the weapon rotation
        private Transform playerPointToLookAt;

        private SecurityWeaponStateMachine securityWeaponStateMachine;

        protected virtual void Awake() {
            securityWeaponStateMachine = GetComponent<SecurityWeaponStateMachine>();
            securityWeaponStateMachine.Init(this, SecurityWeaponStateType.Shutdown);
        }


        // To creat projectiles and shoot them towards the target
        public abstract void Shoot(IDamageable target);

        public abstract void Reload(int ammoNumber);

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

        public void UpdateWeaponStrength() {
            weaponLevel++;
            print("LeveledUp " + weaponLevel);
        }

        public void TakeDamage(float damageCost) {
            weaponHealth -= damageCost;
            weaponHealthBar.normalizedValue = weaponHealth / initialHealth;

            if (weaponHealth <= 0) {
                weaponHealthBar.normalizedValue = 0;
                weaponHealth = 0;
                gameObject.SetActive(false);
                Ctx.Deps.EventsManager.OnSecurityWeaponDestroy();
            }
        }

        [Serializable]
        private class Sound {
            public AudioClip audioClip;
            public float volume;
        }


    }
}