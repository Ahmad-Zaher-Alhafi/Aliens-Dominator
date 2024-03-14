using AmmoMagazines;
using Context;
using FiniteStateMachine;
using Unity.Netcode;
using UnityEngine;

namespace SecurityWeapons {
    public abstract class DefenceWeapon : NetworkBehaviour, IWeaponSpecification, IAutomatable {
        [SerializeField] private AutomationCommander automationCommander;
        protected AutomationCommander AutomationCommander => automationCommander;

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

        protected virtual void Awake() {
            Ctx.Deps.EventsManager.AmmoSuppliesCollected += OnAmmoSuppliesCollected;
        }

        protected abstract void OnAmmoSuppliesCollected(Magazine.AmmoType ammoType, int ammoNumber);

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.AmmoSuppliesCollected -= OnAmmoSuppliesCollected;
        }
    }
}