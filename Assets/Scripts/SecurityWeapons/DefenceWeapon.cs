using FiniteStateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SecurityWeapons {
    public abstract class DefenceWeapon : NetworkBehaviour, IWeaponSpecification, IAutomatable {
        public enum WeaponType {
            Ground,
            Air,
            FighterPlane
        }

        [Header("Specifications")]
        [Tooltip("Min/Max angel that the weapon can rotate around y axis")]
        [SerializeField] private Vector2 rotateOnYAxisRange;
        public Vector3 RotateOnYAxisRange => rotateOnYAxisRange;

        [Tooltip("Min/Max angel that the weapon can rotate around x axis")]
        [SerializeField] private Vector2 rotateOnXAxisRange;
        public Vector3 RotateOnXAxisRange => rotateOnXAxisRange;
        [SerializeField] private bool activeOnStart;
        protected bool ActiveOnStart => activeOnStart;

        public GameObject GameObject => gameObject;
        public virtual bool IsDestroyed => false;
        public virtual bool IsAutomatingEnabled { get; set; } = true;
        public Quaternion InitialRotation { get; set; }

        protected virtual void Awake() { }

        public void Init() {
            InitialRotation = transform.rotation;
        }
    }
}