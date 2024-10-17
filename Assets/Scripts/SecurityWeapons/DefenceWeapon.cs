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
    public abstract class DefenceWeapon : NetworkBehaviour, IWeaponSpecification, IAutomatable, IHighlightable {
        public enum WeaponsType {
            Ground,
            Air,
            FighterPlane
        }

        [SerializeField] private WeaponsType weaponType;
        public WeaponsType WeaponType => weaponType;

        [SerializeField] private bool activeOnStart;
        protected bool ActiveOnStart => activeOnStart;

        [Header("Outline")]
        [SerializeField] private Outline outline;

        public abstract Vector3 RotateOnYAxisRange { get; }
        public abstract Vector3 RotateOnXAxisRange { get; }
        public abstract float Range { get; }
        public GameObject GameObject => gameObject;
        public virtual bool IsDestroyed => false;
        public virtual bool IsAutomatingEnabled { get; set; } = true;
        public Quaternion InitialRotation { get; set; }

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

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
        }

        public void Despawn() {
            foreach (NetworkObject networkObject in GetComponentsInChildren<NetworkObject>().Where(netObj => netObj != NetworkObject)) {
                networkObject.transform.SetParent(null);
                networkObject.Despawn();
            }

            NetworkObject.Despawn();
        }
    }
}