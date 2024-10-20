using AmmoMagazines;
using FiniteStateMachine;
using FiniteStateMachine.SecurityWeaponMachine;
using UnityEditor;
using UnityEngine;

namespace SecurityWeapons {
    public abstract class SecurityWeapon<TEnemyType> : MonoBehaviour, IWeaponSpecification, IAutomatable where TEnemyType : IAutomatable {
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public Vector3 InitialEulerAngels { get; private set; }
        public SecurityWeaponStateType CurrentStateType => securityWeaponStateMachine.PrimaryState.Type;
        public bool IsDestroyed => weaponHealth <= 0;

        [SerializeField] private WeaponSensor<TEnemyType> weaponSensor;
        public WeaponSensor<TEnemyType> WeaponSensor => weaponSensor;

        [Header("Specifications")]
        [Tooltip("Min/Max angel that the weapon can rotate around y axis")]
        [SerializeField] private Vector2 rotateOnYAxisRange;
        [Tooltip("Min/Max angel that the weapon can rotate around x axis")]
        [SerializeField] private Vector2 rotateOnXAxisRange;
        public Vector3 RotateOnYAxisRange => rotateOnYAxisRange;
        public Vector3 RotateOnXAxisRange => rotateOnXAxisRange;
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

        protected Magazine Magazine;
        private SecurityWeaponStateMachine<TEnemyType> securityWeaponStateMachine;

        protected virtual void Awake() {
            InitialEulerAngels = transform.eulerAngles;
            Magazine = GetComponent<Magazine>();
            securityWeaponStateMachine = GetComponent<SecurityWeaponStateMachine<TEnemyType>>();
            securityWeaponStateMachine.Init(this, SecurityWeaponStateType.Shutdown);
        }

        private void Update() {
#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        public abstract void Shoot(IDamageable target);

        private void Reload(int ammoNumberToAdd) {
            Magazine.Refill(ammoNumberToAdd);
        }


#if UNITY_EDITOR
        [Header("Editor stuff")]
        [SerializeField, HideInInspector] private bool useInfiniteAmmo;
        private void EditorUpdate() {
            if (useInfiniteAmmo && Magazine.IsEmpty) {
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