using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class GoingBackToBaseState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.GoingBackToBase;
        public override bool CanBeActivated() => AutomatedObject.HasToGoBackToBase;

        /// <summary>
        /// It reached the landing point only if it has the almost the same x and z position of the landing point
        /// </summary>
        private bool ReachedLandingPoint => Mathf.Abs(AutomatedObject.transform.position.x - AutomatedObject.LandingPoint.position.x) <= .5f &&
                                            Mathf.Abs(AutomatedObject.transform.position.z - AutomatedObject.LandingPoint.position.z) <= .5f;

        public GoingBackToBaseState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Tick() {
            base.Tick();
            GetBackToBase();
        }

        private void GetBackToBase() {
            // Get back to the landing point
            if (!ReachedLandingPoint) {
                AutomatedObject.transform.position = Vector3.MoveTowards(AutomatedObject.transform.position, AutomatedObject.LandingPoint.position, AutomatedObject.TakeOffSpeed * Time.deltaTime);
                RotateTowards(AutomatedObject.LandingPoint.position);
                return;
            }

            // Get down to the take off point
            if (Vector3.Distance(AutomatedObject.transform.position, AutomatedObject.TakeOffPoint.position) >= .5f) {
                AutomatedObject.transform.position = Vector3.MoveTowards(AutomatedObject.transform.position, AutomatedObject.TakeOffPoint.position, AutomatedObject.TakeOffSpeed * Time.deltaTime);
                Rotate(AutomatedObject.InitialRotation);
                return;
            }

            Fulfil();
        }

        private void RotateTowards(Vector3 position) {
            Quaternion targetRotation = Quaternion.LookRotation(position);
            AutomatedObject.transform.rotation = Quaternion.Slerp(AutomatedObject.transform.rotation, targetRotation, AutomatedObject.RotateSpeed);
        }
        
        private void Rotate(Quaternion rotation) {
            AutomatedObject.transform.rotation = Quaternion.SlerpUnclamped(AutomatedObject.transform.rotation, rotation, .1f);
        }

        public override void Fulfil() {
            base.Fulfil();
            AutomatedObject.HasLanded = true;
            AutomatedObject.HasToGoBackToBase = false;
        }
    }
}