using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class AimingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Aiming;
        public override bool CanBeActivated() => AutomatedObject.WeaponSensor.TargetToAimAt != null;

        public AimingState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Tick() {
            base.Tick();
            LookTowardsTarget();
        }

        private void LookTowardsTarget() {
            if (AutomatedObject.WeaponSensor.TargetToAimAt == null) {
                Fulfil();
                return;
            }

            // Get the current rotation of the object
            Quaternion currentRotation = AutomatedObject.transform.rotation;

            // Get the target rotation
            Vector3 targetDirection = AutomatedObject.WeaponSensor.TargetToAimAt.GameObject.transform.position - AutomatedObject.transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (Quaternion.Angle(currentRotation, targetRotation) > 1) {
                // Smoothly rotate towards the target point
                AutomatedObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, AutomatedObject.AimingSpeed * Time.deltaTime);
            } else {
                Fulfil();
            }
        }
    }
}