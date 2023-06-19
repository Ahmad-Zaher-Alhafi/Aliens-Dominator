using SecurityWeapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GuardingState : SecurityWeaponState {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Guarding;
        public override bool CanBeActivated() => !isBusy && StateObject.WeaponSensor.TargetToAimAt == null;

        private bool isBusy;
        private Vector3 targetPosition;

        public GuardingState(SecurityWeapon securityWeapon) : base(securityWeapon) { }

        public override void Tick() {
            base.Tick();
            Guard();
        }

        public override void Activate() {
            base.Activate();
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
            Quaternion currentRotation = StateObject.transform.rotation;

            // Get the target rotation
            Vector3 targetDirection = targetPosition - StateObject.transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (Quaternion.Angle(currentRotation, targetRotation) > .2f) {
                // Smoothly rotate towards the target point
                StateObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, StateObject.GuardingSpeed * Time.deltaTime);
            } else {
                Fulfil();
            }
        }

        private Vector3 GetRandomTargetPosition() {
            //-1 or 1 for negative or positive
            int direction = Random.Range(0, 2) * 2 - 1;
            Vector3 xPosition = StateObject.transform.right * (Random.Range(StateObject.GuardingXRange.x, StateObject.GuardingXRange.y) * direction);

            // Air security weapon can not look down
            if (StateObject is AirSecurityWeapon) {
                direction = 1;
            } else {
                direction = Random.Range(0, 2) * 2 - 1;
            }

            Vector3 yPosition = StateObject.transform.up * (Random.Range(StateObject.GuardingYRange.x, StateObject.GuardingYRange.y) * direction);

            // Air security weapon can rotate 360 on z axis
            if (StateObject is AirSecurityWeapon) {
                direction = Random.Range(0, 2) * 2 - 1;
            } else {
                direction = 1;
            }

            Vector3 zPosition = StateObject.transform.forward * (100 * direction);

            return xPosition + yPosition + zPosition;
        }
    }
}