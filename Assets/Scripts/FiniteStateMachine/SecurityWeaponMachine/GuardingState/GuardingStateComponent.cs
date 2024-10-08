using SecurityWeapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.SecurityWeaponMachine.GuardingState {
    public class GuardingStateComponent : StateComponent {
        [SerializeField] private float guardingSpeed = 10;

        private Quaternion targetRotation;
        private DefenceWeapon defenceWeapon;


        protected override void Awake() {
            base.Awake();
            defenceWeapon = GetComponent<DefenceWeapon>();
        }

        protected override void OnStateActivationChanged(bool isActive) {
            base.OnStateActivationChanged(isActive);
            if (!isActive) return;

            GenerateRandomRotation();
        }

        private void Update() {
            if (!State.IsActive) return;
            Guard();
        }

        // To let the weapon rotates randomly (escorting area)
        private void Guard() {
            if (transform.rotation != targetRotation) {
                // Smoothly rotate towards the target rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, guardingSpeed * Time.deltaTime);
            } else {
                State.Fulfil();
            }
        }

        private void GenerateRandomRotation() {
            Vector3 initialEulerAngels = defenceWeapon.InitialRotation.eulerAngles;

            // Get random angel on X axis
            Vector3 targetEulerAngels = Vector3.right * (initialEulerAngels.x + Random.Range(defenceWeapon.RotateOnXAxisRange.x, defenceWeapon.RotateOnXAxisRange.y));
            // Get random angel on Y axis
            targetEulerAngels += Vector3.up * (initialEulerAngels.y + Random.Range(defenceWeapon.RotateOnYAxisRange.x, defenceWeapon.RotateOnYAxisRange.y));
            // Give the same angel on Z axis
            targetEulerAngels += Vector3.forward * defenceWeapon.transform.eulerAngles.z;

            // Return it as quaternion to avoid the gimbal lock of the euler angels
            targetRotation = Quaternion.Euler(targetEulerAngels);
        }
    }
}