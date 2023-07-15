using System.Collections.Generic;
using Context;
using Creatures;
using FiniteStateMachine;
using UnityEngine;
using Utils;

namespace SecurityWeapons {
    public abstract class WeaponSensor<TEnemyType> : MonoBehaviour where TEnemyType : IAutomatable {
        [SerializeField] private float sensorRange = 150;
        public IDamageable TargetToAimAt { get; protected set; }

        public IReadOnlyList<Creature> Targets;

        protected readonly List<IDamageable> targets = new();
        protected IWeaponSpecification weaponSpecification;
        private RaycastHit[] raycastHits = new RaycastHit[10];

        private void Awake() {
            weaponSpecification = GetComponentInParent<IWeaponSpecification>();
        }

        private void Update() {
            if (!Ctx.Deps.CreatureSpawnController.HasWaveStarted) return;

            if (CanBeShot(TargetToAimAt)) return;

            TargetToAimAt = null;
            // Find new target if the current one is no longer shoot able
            FindNewTarget();
        }

        private bool CanBeShot(IDamageable target) {
            if (target == null || target.IsDestroyed) return false;

            if (!IsInRange(target.GameObject)) return false;

            if (!CanRotateTowards(target.GameObject)) return false;

            if (IsHidingBehindSomething(target.GameObject)) return false;

            Debug.DrawRay(transform.position, target.GameObject.transform.position - transform.position, Color.red);
            return true;
        }

        /// <summary>
        /// True if the weapon angel does not exceed the angel between the weapon and the target
        /// </summary>
        /// <returns></returns>
        private bool CanRotateTowards(GameObject target) {
            // Get the current rotation of the object
            Quaternion currentRotation = transform.rotation;

            // Get the target rotation
            Vector3 targetDirection = target.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            return Quaternion.Angle(currentRotation, targetRotation) >= weaponSpecification.RotateYRange.x && Quaternion.Angle(currentRotation, targetRotation) <= weaponSpecification.RotateYRange.y;

        }

        // Find new target to aim at
        private void FindNewTarget() {
            foreach (IDamageable target in targets) {
                bool canBeShot = CanBeShot(target);
                if (canBeShot) {
                    TargetToAimAt = target;
                }
            }
        }

        private bool IsInRange(GameObject target) {
            return Vector3.Distance(target.transform.position, transform.position) <= sensorRange;
        }

        private bool IsHidingBehindSomething(GameObject target) {
            if (Physics.RaycastNonAlloc(transform.position,
                    target.transform.position - transform.position, raycastHits,
                    Vector3.Distance(target.transform.position, transform.position),
                    ~LayerMask.GetMask("Ignore Raycast")) == 0) return true;

            return raycastHits[0].collider.gameObject.layer != LayerMask.NameToLayer("Enemy");
        }
        
        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.layer != Constants.ENEMY_LAYER_ID) return;

            Creature creature = other.GetComponentInParent<Creature>();

            if (creature is not TEnemyType) return;

            BodyPart bodyPart = MathUtils.GetRandomObjectFromList(creature.BodyParts);
            if (targets.Contains(bodyPart)) return;

            targets.Add(bodyPart);
        }

        private void OnTriggerExit(Collider other) {
            if (other.gameObject.layer != Constants.ENEMY_LAYER_ID) return;

            BodyPart bodyPart = other.GetComponent<BodyPart>();
            if (bodyPart == null) return;

            if (!targets.Contains(bodyPart)) return;

            targets.Remove(bodyPart);
            if (bodyPart == (BodyPart) TargetToAimAt) {
                TargetToAimAt = null;
            }
        }
    }
}