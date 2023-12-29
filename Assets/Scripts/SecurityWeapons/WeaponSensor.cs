using System.Collections.Generic;
using Context;
using Creatures;
using FiniteStateMachine;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using BodyPart = Creatures.BodyPart;
using MathUtils = Utils.MathUtils;

namespace SecurityWeapons {
    public abstract class WeaponSensor<TEnemyType> : MonoBehaviour where TEnemyType : IAutomatable {
        [SerializeField] private float sensorRange = 150;
        [SerializeField] private bool activateDebugging;
        public IDamageable TargetToAimAt { get; private set; }

        private readonly List<IDamageable> targets = new();
        private IWeaponSpecification weaponSpecification;
        private readonly RaycastHit[] raycastHits = new RaycastHit[10];
        private bool hasToDefend;

        /// <summary>
        /// The transform.up as it was set in the editor before the sensor rotates
        /// </summary>
        private Vector3 initialUpVector;
        /// <summary>
        /// The transform.forward as it was set in the editor before the sensor rotates
        /// </summary>
        private Vector3 initialForwardVector;
        /// <summary>
        /// The transform.right as it was set in the editor before the sensor rotates
        /// </summary>
        private Vector3 initialRightVector;

        private void Awake() {
            weaponSpecification = GetComponentInParent<IWeaponSpecification>();
            Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
            initialUpVector = transform.up;
            initialForwardVector = transform.forward;
            initialRightVector = transform.right;
        }

        private void OnWaveStarted(Wave wave) {
            hasToDefend = true;
        }

        private void Update() {
            if (!hasToDefend) return;

            if (CanBeShot(TargetToAimAt)) return;

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
        /// True if the required rotation does not exceed the rotation range of the weapon
        /// </summary>
        /// <returns></returns>
        private bool CanRotateTowards(GameObject target) {
            // Get the target direction
            Vector3 targetDirection = target.transform.position - transform.position;
            Vector3 targetForward = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;

            float signedXAngel = Vector3.SignedAngle(targetForward, targetDirection, initialRightVector);
            float dotProduct = Vector3.Dot(targetDirection, initialForwardVector);

            if (dotProduct < 0) {
                // The target is behind the weapon, so negate the angle
                // This is needed because the signed angel is being returned as positive when the target is behind the initial right vector
                signedXAngel = -signedXAngel;
            }

            // Project the vectors onto the xz plane (to prevent the angel from getting affected by the change of the target position on y axis)
            Vector3 from = Vector3.ProjectOnPlane(initialForwardVector, initialUpVector);
            Vector3 to = Vector3.ProjectOnPlane(targetDirection, initialUpVector);
            float signedYAngel = Vector3.SignedAngle(from, to, initialUpVector);

            if (activateDebugging) {
                Debug.LogWarning($"Signed angel : {signedXAngel},{signedYAngel}");
                Debug.DrawRay(transform.position, initialForwardVector * dotProduct * 100, Color.magenta);
                Debug.DrawRay(transform.position, initialRightVector * 100, Color.red);
                Debug.DrawRay(transform.position, initialForwardVector * 100, Color.blue);
            }

            // Check if the rotation does not exceed the rotation range of the weapon
            return weaponSpecification.RotateOnXAxisRange.x <= signedXAngel && signedXAngel <= weaponSpecification.RotateOnXAxisRange.y &&
                   weaponSpecification.RotateOnYAxisRange.x <= signedYAngel && signedYAngel <= weaponSpecification.RotateOnYAxisRange.y;
        }

        // Find new target to aim at
        private void FindNewTarget() {
            TargetToAimAt = null;
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
            if (bodyPart == TargetToAimAt as BodyPart) {
                TargetToAimAt = null;
            }
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(WeaponSensor<>))]
        public class WeaponSensorEditor : Editor {
            private Creature testCreature;

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                WeaponSensor<TEnemyType> weaponSensor = (WeaponSensor<TEnemyType>) target;

                // Use test target
                if (GUILayout.Button("Use test target")) {
                    if (Application.isPlaying) {
                        weaponSensor.hasToDefend = true;
                        testCreature ??= GameObject.FindGameObjectWithTag("TestCreature").GetComponent<Creature>();
                        weaponSensor.TargetToAimAt = testCreature;
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}