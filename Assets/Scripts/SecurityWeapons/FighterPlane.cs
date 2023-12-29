using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmmoMagazines;
using Context;
using Creatures;
using FiniteStateMachine;
using FiniteStateMachine.FighterPlaneStateMachine;
using FMODUnity;
using Projectiles;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public class FighterPlane : MonoBehaviour, IAutomatable, IWeaponSpecification {
        public FighterPlaneStateType CurrentStateType => fighterPlaneStateMachine.PrimaryState.Type;
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsDestroyed => false;
        public bool HasToTakeOff { get; set; }
        public bool HasToGoBackToBase { get; set; }
        public bool HasLanded { get; set; }


        [Header("Specifications")]
        [SerializeField] private Vector2 rotateOnYAxisRange;
        [SerializeField] private Vector2 rotateOnXAxisRange;
        public Vector3 RotateOnYAxisRange => rotateOnYAxisRange;
        public Vector3 RotateOnXAxisRange => rotateOnXAxisRange;

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

        private void Awake() {
            magazines = GetComponents<Magazine>().ToList();
            fighterPlaneStateMachine = GetComponent<FighterPlaneStateMachine>();
            fighterPlaneStateMachine.Init(this, FighterPlaneStateType.Deactivated);
        }

        private void Update() {
#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        public void Shoot(Type ammoType, IDamageable target) {
            magazines.Single(magazine => magazine.AmmoType == ammoType).GetProjectile()?.Fire(target, hasToUseRockets ? null : bulletCreatePoint);
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

            PlayTakeOffSound();
            foreach (var particles in smokeParticles) {
                particles.Play();
            }
        }

        public void GetBackToBase() {
            if (IsDestroyed) return;
            HasToGoBackToBase = true;

            PlayLandSound();
            foreach (var particles in smokeParticles) {
                particles.Stop();
            }
        }

        private void PlayTakeOffSound() {
            takeOffSound.Play();
        }

        private void PlayLandSound() {
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

            if (useInfiniteAmmo && magazines.Any(magazine => magazine.IsEmpty)) {
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