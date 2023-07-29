using SecurityWeapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GuardingState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Guarding;
        public override bool CanBeActivated() => !isBusy && AutomatedObject.WeaponSensor.TargetToAimAt == null;

        private bool isBusy;
        private Quaternion targetRotation;

        public GuardingState(SecurityWeapon<TEnemyType> securityWeapon) : base(securityWeapon) { }

        public override void Tick() {
            base.Tick();
            Guard();
        }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            targetRotation = GetRandomRotation();
            isBusy = true;
        }

        public override void Fulfil() {
            base.Fulfil();
            isBusy = false;
        }

        public override void Interrupt() {
            base.Interrupt();
            isBusy = false;
        }

        // To let the weapon rotates randomly (escorting area)
        private void Guard() {
            if (AutomatedObject.transform.rotation != targetRotation) {
                // Smoothly rotate towards the target rotation
                AutomatedObject.transform.rotation = Quaternion.RotateTowards(AutomatedObject.transform.rotation, targetRotation, AutomatedObject.GuardingSpeed * Time.deltaTime);
            } else {
                Fulfil();
            }
        }

        private Quaternion GetRandomRotation() {
            Vector3 initialEulerAngels = AutomatedObject.InitialEulerAngels;

            // Get random angel on X axis
            Vector3 targetEulerAngels = Vector3.right * (initialEulerAngels.x + Random.Range(AutomatedObject.RotateOnXAxisRange.x, AutomatedObject.RotateOnXAxisRange.y));
            // Get random angel on Y axis
            targetEulerAngels += Vector3.up * (initialEulerAngels.y + Random.Range(AutomatedObject.RotateOnYAxisRange.x, AutomatedObject.RotateOnYAxisRange.y));
            // Give the same angel on Z axis
            targetEulerAngels += Vector3.forward * AutomatedObject.transform.eulerAngles.z;
            
            // Return it as quaternion to avoid the gimbal lock of the euler angels
            return Quaternion.Euler(targetEulerAngels);
        }
    }
}