using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class TakingOffState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.TakingOff;
        public override bool CanBeActivated() => AutomatedObject.HasToTakeOff;

        public TakingOffState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.HasLanded = false;
            AutomatedObject.HasToTakeOff = false;
        }

        public override void Tick() {
            base.Tick();
            TakeOff();
        }

        private void TakeOff() {
            if (Vector3.Distance(AutomatedObject.transform.position, AutomatedObject.LandingPoint.position) >= .5f) {
                AutomatedObject.transform.position = Vector3.MoveTowards(AutomatedObject.transform.position, AutomatedObject.LandingPoint.position, AutomatedObject.TakeOffSpeed * Time.deltaTime);
                return;
            }

            Fulfil();
        }
    }
}