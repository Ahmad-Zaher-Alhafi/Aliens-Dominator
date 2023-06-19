using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class AimingState : SecurityWeaponState {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Aiming;
        public override bool CanBeActivated() => StateObject.WeaponSensor.TargetToAimAt != null;

        public AimingState(SecurityWeapon securityWeapon) : base(securityWeapon) { }

        public override void Tick() {
            base.Tick();
            LookTowardsTarget();
        }

        private void LookTowardsTarget() {
            if (StateObject.WeaponSensor.TargetToAimAt == null) {
                Fulfil();
                return;
            }

            // Get the current rotation of the object
            Quaternion currentRotation = StateObject.transform.rotation;

            // Get the target rotation
            Vector3 targetDirection = StateObject.WeaponSensor.TargetToAimAt.GameObject.transform.position - StateObject.transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (Quaternion.Angle(currentRotation, targetRotation) > 1) {
                // Smoothly rotate towards the target point
                StateObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, StateObject.AimingSpeed * Time.deltaTime);
            } else {
                Fulfil();
            }
        }
    }
}