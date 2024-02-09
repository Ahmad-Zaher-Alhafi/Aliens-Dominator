using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmmoMagazines;
using Context;
using Creatures;
using FiniteStateMachine.FighterPlaneStateMachine;
using FMODUnity;
using Projectiles;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class FighterPlane : DefenceWeapon {
        public FighterPlaneStateType CurrentStateType => fighterPlaneStateMachine.PrimaryState.Type;
        public bool HasToTakeOff { get; set; }
        public bool HasToGoBackToBase { get; set; }
        public bool HasLanded {
            get => hasLanded;
            set {
                hasLanded = value;
                if (hasLanded) {
                    StopSmokeParticlesClientRPC();
                    PlayLandSoundClientRPC();
                }
            }
        }
        private bool hasLanded;
        public override bool IsAutomatingEnabled {
            get => isAutomatingEnabled;
            set {
                isAutomatingEnabled = value;
                fighterPlaneStateMachine.OnAutomationStatusChanged();
            }
        }
        private bool isAutomatingEnabled;

        [Header("Speeds")]
        [SerializeField] private float patrollingSpeed;
        public float PatrollingSpeed => patrollingSpeed;

        [SerializeField] private float rotateSpeed = 2;
        public float RotateSpeed => rotateSpeed;

        [SerializeField] private float aimingSpeed = 15;
        public float AimingSpeed => aimingSpeed;

        [SerializeField] private float takeOffSpeed;
        public float TakeOffSpeed => takeOffSpeed;

        [Header("Shooting and ammo")]
        [SerializeField] private bool hasToUseRockets;
        public bool HasToUseRockets => hasToUseRockets;

        [SerializeField] private Transform bulletCreatePoint;

        [SerializeField] private float bulletsPerSecond = 4;
        public float BulletsPerSecond => bulletsPerSecond;

        [SerializeField] private float rocketsPerSecond = .5f;

        [SerializeField] private bool useBursts;
        [SerializeField] private int numOfRocketsInBurst;
        [SerializeField] private float rocketsPerSecondInBurst;
        [SerializeField] private float burstCoolDown = 1.5f;


        [Header("Flying Points")]
        [SerializeField] private Transform takeOffPoint;
        /// <summary>
        /// Point where the plane land and take off from
        /// </summary>
        public Transform TakeOffPoint => takeOffPoint;

        [SerializeField] private Transform landingPoint;
        /// <summary>
        /// The point above the plane base where the plane should go to before stars landing
        /// </summary>
        public Transform LandingPoint => landingPoint;

        [SerializeField] private List<Transform> patrollingPoints = new();
        public IReadOnlyList<Transform> PatrollingPoints => patrollingPoints;

        [Header("Audio")]
        [SerializeField] private StudioEventEmitter takeOffSound;
        [SerializeField] private StudioEventEmitter landSound;
        [SerializeField] private StudioEventEmitter bulletSound;

        [Header("Others")]
        [SerializeField] private ParticleSystem[] smokeParticles;

        [SerializeField] private WeaponSensor<Creature> weaponSensor;
        public WeaponSensor<Creature> WeaponSensor => weaponSensor;
        public Quaternion InitialRotation { get; private set; }

        private int currentBulletsNumber;

        private IDamageable target;
        private FighterPlaneStateMachine fighterPlaneStateMachine;
        private List<Magazine> magazines = new();

        private int numOfRocketsShotInBurst;
        private bool isCoolingDown;

        public float RocketsPerSecond {
            get {
                if (useBursts) {
                    return rocketsPerSecondInBurst;
                }

                numOfRocketsShotInBurst = 0;
                return rocketsPerSecond;
            }
        }

        public float CoolDownTime {
            get {
                if (useBursts && numOfRocketsShotInBurst == numOfRocketsInBurst) {
                    if (!isCoolingDown) {
                        StartCoroutine(CoolDown());
                    }
                    return burstCoolDown;
                }

                return 0;
            }
        }

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) return;

            Destroy(fighterPlaneStateMachine);
            foreach (Magazine magazine in magazines) {
                Destroy(magazine);
            }
        }

        protected override void Awake() {
            base.Awake();
            magazines = GetComponents<Magazine>().ToList();
            fighterPlaneStateMachine = GetComponent<FighterPlaneStateMachine>();
            fighterPlaneStateMachine.Init(this, FighterPlaneStateType.Deactivated);
            InitialRotation = transform.rotation;
        }

        private void Update() {
            if (IsSpawned) {
                if (IsServer) {
                    networkPosition.Value = transform.position;
                    networkRotation.Value = transform.rotation;
                } else {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
                }
            }

#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        public void Shoot(Type ammoType, IDamageable target) {
            Projectile projectile = magazines.Single(magazine => magazine.AmmoType == ammoType).GetProjectile();
            if (projectile == null) {
                Debug.Log($"Fighter plane has ran away of {ammoType}s");
                return;
            }

            projectile.Fire(target, hasToUseRockets ? null : bulletCreatePoint);

            if (ammoType == typeof(Rocket) && useBursts) {
                numOfRocketsShotInBurst++;
            }

            if (ammoType == typeof(Bullet)) {
                bulletSound.Play();
            }
        }

        private void Reload(Type ammoType, int ammoNumberToAdd) {
            magazines.Single(magazine => magazine.AmmoType == ammoType).Refill(ammoNumberToAdd);
        }

        public void TakeOff() {
            if (HasToTakeOff) return;
            if (IsDestroyed) return;
            HasToTakeOff = true;

            PlayTakeOffSoundClientRPC();
            PlaySmokeParticlesClientRPC();
        }

        public void GetBackToBase() {
            if (IsDestroyed) return;
            HasToGoBackToBase = true;
        }

        [ClientRpc]
        private void PlaySmokeParticlesClientRPC() {
            foreach (var particles in smokeParticles) {
                particles.Play();
            }
        }

        [ClientRpc]
        private void StopSmokeParticlesClientRPC() {
            foreach (var particles in smokeParticles) {
                particles.Stop();
            }
        }

        [ClientRpc]
        private void PlayTakeOffSoundClientRPC() {
            landSound.Stop();
            takeOffSound.Play();
        }

        [ClientRpc]
        private void PlayLandSoundClientRPC() {
            takeOffSound.Stop();
            landSound.Play();
        }

        private IEnumerator CoolDown() {
            isCoolingDown = true;
            yield return new WaitForSeconds(burstCoolDown);
            numOfRocketsShotInBurst = 0;
            isCoolingDown = false;
        }


#if UNITY_EDITOR
        [Header("Editor stuff")]
        [SerializeField, HideInInspector]
        private bool activateOnStart;
        [SerializeField, HideInInspector]
        private bool useInfiniteAmmo;

        private void EditorUpdate() {
            if (activateOnStart && Ctx.Deps.WaveController.HasWaveStarted) {
                TakeOff();
            }

            if (IsServer && useInfiniteAmmo && magazines.Any(magazine => magazine.IsEmpty)) {
                foreach (Magazine magazine in magazines) {
                    magazine.Refill(20);
                }
            }
        }

        [CustomEditor(typeof(FighterPlane))]
        public class FighterPlaneEditor : Editor {
            private SerializedProperty activateOnStart;
            private SerializedProperty useInfiniteAmmo;

            private void OnEnable() {
                // Find the serialized property by name
                activateOnStart = serializedObject.FindProperty("activateOnStart");
                useInfiniteAmmo = serializedObject.FindProperty("useInfiniteAmmo");
            }

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                FighterPlane fighterPlane = (FighterPlane) target;

                EditorGUILayout.Space();

                // Update the serialized object
                serializedObject.Update();

                // Show a checkbox using the serialized property
                EditorGUILayout.PropertyField(activateOnStart, new GUIContent("Take off on wave started"));
                EditorGUILayout.PropertyField(useInfiniteAmmo, new GUIContent("Infinite ammo"));

                // Apply the changes to the serialized object
                serializedObject.ApplyModifiedProperties();

                // Activate the plane button
                if (GUILayout.Button("Activate plane")) {
                    if (Application.isPlaying) {
                        fighterPlane.TakeOff();
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}