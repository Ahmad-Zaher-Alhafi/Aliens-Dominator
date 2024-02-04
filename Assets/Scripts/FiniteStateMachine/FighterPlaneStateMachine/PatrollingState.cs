using SecurityWeapons;
using UnityEngine;
using Utils;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class PatrollingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Patrolling;
        public override bool CanBeActivated() => !IsActive;

        private Transform randomPatrolPoint;

        public PatrollingState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Tick() {
            base.Tick();
            LookTowardsTarget();
            Patrol();
        }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            randomPatrolPoint = MathUtils.GetRandomObjectFromList(AutomatedObject.PatrollingPoints);
        }

        private void Patrol() {
            // Move to the random point
            if (Vector3.Distance(randomPatrolPoint.position, AutomatedObject.transform.position) >= .5f) {
                AutomatedObject.transform.position = Vector3.MoveTowards(AutomatedObject.transform.position, randomPatrolPoint.position, AutomatedObject.PatrollingSpeed * Time.deltaTime);
                return;
            }

            Fulfil();
        }

        private void LookTowardsTarget() {
            // Do not rotate to the patrolling point if it's aiming at a target
            if (AutomatedObject.WeaponSensor.TargetToAimAt != null) return;

            // Get the current rotation of the object
            Quaternion currentRotation = AutomatedObject.transform.rotation;

            // Get the target rotation
            Vector3 targetDirection = randomPatrolPoint.position - AutomatedObject.transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (Quaternion.Angle(currentRotation, targetRotation) > 1) {
                // Smoothly rotate towards the target point
                AutomatedObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, AutomatedObject.RotateSpeed * Time.deltaTime);
            }
        }
    }
}