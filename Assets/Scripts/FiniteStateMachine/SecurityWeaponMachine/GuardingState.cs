using SecurityWeapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GuardingState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Guarding;
        public override bool CanBeActivated() => !isBusy && AutomatedObject.WeaponSensor.TargetToAimAt == null;

        private bool isBusy;
        private Vector3 targetPosition;

        public GuardingState(SecurityWeapon<TEnemyType> securityWeapon) : base(securityWeapon) { }

        public override void Tick() {
            base.Tick();
            Guard();
        }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            targetPosition = GetRandomTargetPosition();
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

        // To let the weapon look towards random points (escorting area)
        private void Guard() {

            // Get the current rotation of the object
            Quaternion currentRotation = AutomatedObject.transform.rotation;

            // Get the target rotation
            Vector3 targetDirection = targetPosition - AutomatedObject.transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (Quaternion.Angle(currentRotation, targetRotation) > .2f) {
                // Smoothly rotate towards the target point
                AutomatedObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, AutomatedObject.GuardingSpeed * Time.deltaTime);
            } else {
                Fulfil();
            }
        }

        private Vector3 GetRandomTargetPosition() {
            //-1 or 1 for negative or positive
            int direction = Random.Range(0, 2) * 2 - 1;
            Vector3 xPosition = AutomatedObject.transform.right * (Random.Range(AutomatedObject.RotateXRange.x, AutomatedObject.RotateXRange.y) * direction);

            // Air security weapon can not look down
            if (AutomatedObject is AirSecurityWeapon) {
                direction = 1;
            } else {
                direction = Random.Range(0, 2) * 2 - 1;
            }

            Vector3 yPosition = AutomatedObject.transform.up * (Random.Range(AutomatedObject.RotateYRange.x, AutomatedObject.RotateYRange.y) * direction);

            // Air security weapon can rotate 360 on z axis
            if (AutomatedObject is AirSecurityWeapon) {
                direction = Random.Range(0, 2) * 2 - 1;
            } else {
                direction = 1;
            }

            Vector3 zPosition = AutomatedObject.transform.forward * (100 * direction);

            return xPosition + yPosition + zPosition;
        }
    }
}