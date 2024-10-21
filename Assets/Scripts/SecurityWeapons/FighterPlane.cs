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
        private bool isAutomatingEnabled = true;

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

        /// <summary>
        /// Point where the plane land and take off from
        /// </summary>
        public Transform TakeOffPoint => Ctx.Deps.PointsController.FighterPlaneTakeOffPoint;
        /// <summary>
        /// The point above the plane base where the plane should go to before stars landing
        /// </summary>
        public Transform LandingPoint => Ctx.Deps.PointsController.FighterPlaneLandingPoint;
        public IReadOnlyList<Transform> PatrollingPoints => Ctx.Deps.PointsController.FighterPlanePatrollingPoints;

        [Header("Audio")]
        [SerializeField] private StudioEventEmitter takeOffSound;
        [SerializeField] private StudioEventEmitter landSound;
        [SerializeField] private StudioEventEmitter bulletSound;

        [Header("Others")]
        [SerializeField] private ParticleSystem[] smokeParticles;

        [SerializeField] private WeaponSensor<Creature> weaponSensor;
        public WeaponSensor<Creature> WeaponSensor => weaponSensor;

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

        public override bool IsDestroyed => IsServer ? IsStateActive<DestroyedState>() : IsDestroyedOnServer;

        public override Vector3 RotateOnXAxisRange => SharedWeaponSpecifications.Instance.FighterPlaneRotateOnXAxisRange;
        public override Vector3 RotateOnYAxisRange => SharedWeaponSpecifications.Instance.FighterPlaneRotateOnYAxisRange;
        public override float Range => SharedWeaponSpecifications.Instance.FighterPlaneRange;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) return;

            Destroy(fighterPlaneStateMachine);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (IsServer) {
                networkPosition.Value = Vector3.zero;
                networkRotation.Value = Quaternion.identity;
            }
        }

        protected override void Awake() {
            base.Awake();
            magazines = GetComponents<Magazine>().ToList();
            fighterPlaneStateMachine = GetComponent<FighterPlaneStateMachine>();
        }

        public override void Init() {
            base.Init();
            fighterPlaneStateMachine.Init(this, FighterPlaneStateType.Deactivated);
        }

        protected override void Update() {
            base.Update();
            if (IsSpawned) {
                if (IsServer) {
                    networkPosition.Value = transform.position;
                    networkRotation.Value = transform.rotation;
                } else {
                    if (networkPosition.Value != Vector3.zero) {
                        transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                        transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
                    }
                }
            }

#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        public void Shoot(Magazine.AmmoType ammoType, IDamageable target) {
            Projectile projectile = magazines.Single(magazine => magazine.TypeOfAmmo == ammoType).GetProjectile(hasToUseRockets ? null : bulletCreatePoint);
            if (projectile == null) {
                Debug.Log($"Fighter plane has ran away of {ammoType}s");
                return;
            }

            projectile.Fire(target);

            if (ammoType == Magazine.AmmoType.Rocket && useBursts) {
                numOfRocketsShotInBurst++;
            }

            if (ammoType == Magazine.AmmoType.Bullet) {
                PlayBulletSoundClientRPC();
            }
        }

        [ClientRpc]
        private void PlayBulletSoundClientRPC() {
            bulletSound.Play();
        }

        public override void Reload(int ammoNumberToAdd, Magazine.AmmoType ammoType = Magazine.AmmoType.Bullet) {
            magazines.Single(magazine => magazine.TypeOfAmmo == ammoType).Refill(ammoNumberToAdd);
        }

        public override void TakeDamage(int damage, Enum damagedPart = null, ulong objectDamagedWithClientID = default) {
            fighterPlaneStateMachine.GetState<GettingHitState>().GotHit(damage);
        }

        public override int GetProjectileAmountInMagazine(Magazine.AmmoType ammoType = Magazine.AmmoType.Bullet) {
            return magazines.Single(magazine => magazine.TypeOfAmmo == ammoType).CurrentProjectilesNumber;
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

        private bool IsStateActive<T>() where T : FighterPlaneState {
            return fighterPlaneStateMachine.GetState<T>().IsActive;
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