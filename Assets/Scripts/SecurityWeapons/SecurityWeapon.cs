using AmmoMagazines;
using FiniteStateMachine;
using FiniteStateMachine.SecurityWeaponMachine;
using Projectiles;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public abstract class SecurityWeapon<TEnemyType> : DefenceWeapon where TEnemyType : IAutomatable {
        public SecurityWeaponStateType CurrentStateType => securityWeaponStateMachine.PrimaryState.Type;
        public override bool IsDestroyed => weaponHealth <= 0;

        [SerializeField] private WeaponSensor<TEnemyType> weaponSensor;
        public WeaponSensor<TEnemyType> WeaponSensor => weaponSensor;


        [SerializeField] private float weaponHealth = 200;

        [Header("Speeds")]
        [SerializeField] private float guardingSpeed;
        public float GuardingSpeed => guardingSpeed;

        [SerializeField] private float aimingSpeed = 15;
        public float AimingSpeed => aimingSpeed;

        [Header("Shooting and ammo")]
        [SerializeField] protected float projectilesPerSecond = 1;
        public virtual float ProjectilesPerSecond => projectilesPerSecond;

        public virtual float CoolDownTime => 0;
        public override bool IsAutomatingEnabled {
            get => isAutomatingEnabled;
            set {
                isAutomatingEnabled = value;
                securityWeaponStateMachine.OnAutomationStatusChanged();
            }
        }
        private bool isAutomatingEnabled = true;


        private Magazine magazine;
        private SecurityWeaponStateMachine<TEnemyType> securityWeaponStateMachine;

        private readonly NetworkVariable<Quaternion> networkRotation = new();


        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) return;

            Destroy(securityWeaponStateMachine);
        }

        protected override void Awake() {
            base.Awake();
            magazine = GetComponent<Magazine>();
            securityWeaponStateMachine = GetComponent<SecurityWeaponStateMachine<TEnemyType>>();
            securityWeaponStateMachine.Init(this, SecurityWeaponStateType.Shutdown);
        }

        private void Update() {
            if (IsSpawned) {
                if (IsServer) {
                    networkRotation.Value = transform.rotation;
                } else {
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
                }
            }

#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        public virtual Projectile Shoot(IDamageable target, Transform spawnPoint = null) {
            Projectile projectile = magazine.GetProjectile(spawnPoint);
            if (projectile == null) {
                Debug.Log($"Weapon {gameObject.name} ran out of ammo!", gameObject);
                return null;
            }

            return projectile;
        }

        public override void Reload(int ammoNumberToAdd, Magazine.AmmoType ammoType = Magazine.AmmoType.Bullet) {
            magazine.Refill(ammoNumberToAdd);
        }

        public override int GetProjectileAmountInMagazine(Magazine.AmmoType ammoType = Magazine.AmmoType.Bullet) {
            return magazine.CurrentProjectilesNumber;
        }


#if UNITY_EDITOR
        [Header("Editor stuff")]
        [SerializeField, HideInInspector] private bool useInfiniteAmmo;
        private void EditorUpdate() {
            if (IsServer && useInfiniteAmmo && magazine.IsEmpty) {
                Reload(16);
            }
        }


        [CustomEditor(typeof(SecurityWeapon<>))]
        public class SecurityWeaponEditor : Editor {
            private SerializedProperty useInfiniteAmmo;

            private void OnEnable() {
                // Find the serialized property by name
                useInfiniteAmmo = serializedObject.FindProperty("useInfiniteAmmo");
            }

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                // Update the serialized object
                serializedObject.Update();

                // Show a checkbox using the serialized property
                EditorGUILayout.PropertyField(useInfiniteAmmo, new GUIContent("Infinite ammo"));

                // Apply the changes to the serialized object
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}