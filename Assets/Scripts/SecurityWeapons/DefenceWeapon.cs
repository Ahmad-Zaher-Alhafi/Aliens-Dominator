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

        [Header("Specifications")]
        [Tooltip("Min/Max angel that the weapon can rotate around y axis")]
        [SerializeField] private Vector2 rotateOnYAxisRange;
        public Vector3 RotateOnYAxisRange => rotateOnYAxisRange;

        [Tooltip("Min/Max angel that the weapon can rotate around x axis")]
        [SerializeField] private Vector2 rotateOnXAxisRange;
        public Vector3 RotateOnXAxisRange => rotateOnXAxisRange;
        [SerializeField] private bool activeOnStart;
        protected bool ActiveOnStart => activeOnStart;

        [Header("Outline")]
        [SerializeField] private Outline outline;
        [SerializeField] private Color normalOutlineColCor;
        [SerializeField] private Color selectionOutlineColCor;

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

        public void HighlightNormal() {
            outline.enabled = true;
            outline.OutlineColor = normalOutlineColCor;
        }

        public void HighlightAsSelected() {
            outline.enabled = true;
            outline.OutlineColor = selectionOutlineColCor;
        }

        public void RemoveHighlight() {
            outline.enabled = false;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
        }
    }
}