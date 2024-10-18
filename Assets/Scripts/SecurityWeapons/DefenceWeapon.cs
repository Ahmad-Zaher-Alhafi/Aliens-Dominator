using System;
using System.Linq;
using AmmoMagazines;
using Context;
using FiniteStateMachine;
using ManagersAndControllers;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace SecurityWeapons {
    [RequireComponent(typeof(Outline))]
    public abstract class DefenceWeapon : NetworkBehaviour, IWeaponSpecification, IAutomatable, IHighlightable, IDamageable {
        public enum WeaponsType {
            Ground,
            Air,
            FighterPlane
        }

        [SerializeField] private WeaponsType weaponType;
        public WeaponsType WeaponType => weaponType;

        [SerializeField] private bool activeOnStart;
        protected bool ActiveOnStart => activeOnStart;

        [SerializeField] private TargetPoint enemyTargetPoint;
        public TargetPoint EnemyTargetPoint => enemyTargetPoint;

        [SerializeField] private int health = 500;
        public int Health {
            get => health;
            private set => health = value;
        }

        [Header("Outline")]
        [SerializeField] private Outline outline;

        public abstract Vector3 RotateOnYAxisRange { get; }
        public abstract Vector3 RotateOnXAxisRange { get; }
        public abstract float Range { get; }
        public GameObject GameObject => gameObject;

        public abstract bool IsDestroyed { get; }
        public virtual bool IsAutomatingEnabled { get; set; } = true;
        public Quaternion InitialRotation { get; set; }
        protected bool IsDestroyedOnServer { get; private set; }

        protected virtual void Awake() {
            if (Ctx.Deps.GameController.CurrentViewMode is not GameController.ViewMode.TopDown) {
                RemoveHighlight();
            } else {
                HighlightNormal();
            }

            Ctx.Deps.EventsManager.ViewModeChanged += OnViewModeChanged;
        }

        private void OnViewModeChanged(GameController.ViewMode previousViewMode, GameController.ViewMode currentViewMode) {
            if (currentViewMode is not GameController.ViewMode.TopDown) {
                RemoveHighlight();
            } else {
                HighlightNormal();
            }
        }

        public void Init() {
            InitialRotation = transform.rotation;
        }

        public abstract void Reload(int ammoNumberToAdd, Magazine.AmmoType ammoType = Magazine.AmmoType.Bullet);
        public abstract void TakeDamage(IDamager damager, int damageWeight, Enum damagedPart = null);

        public void OnDamageTaken(int totalDamage) {
            Health -= totalDamage;
        }

        public void HighlightNormal() {
            outline.enabled = true;
            outline.OutlineColor = SharedWeaponSpecifications.Instance.NormalOutlineColor;
        }

        public void HighlightAsSelected() {
            outline.enabled = true;
            outline.OutlineColor = SharedWeaponSpecifications.Instance.SelectionOutlineColor;
        }

        public void RemoveHighlight() {
            outline.enabled = false;
        }

        public abstract int GetProjectileAmountInMagazine(Magazine.AmmoType ammoType = Magazine.AmmoType.Bullet);

        [ClientRpc]
        private void OnDespawnClientRPC() {
            IsDestroyedOnServer = true;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
        }

        public void Despawn() {
            OnDespawnClientRPC();

            foreach (NetworkObject networkObject in GetComponentsInChildren<NetworkObject>().Where(netObj => netObj != NetworkObject)) {
                networkObject.transform.SetParent(null);
                networkObject.Despawn();
            }

            NetworkObject.Despawn();
        }
    }
}