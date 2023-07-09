using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class TakingOffState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.TakingOff;
        public override bool CanBeActivated() => AutomatedObject.HasToTakeOff;

        public TakingOffState(FighterPlane fighterPlane) : base(fighterPlane) { }

        public override void Tick() {
            base.Tick();
            TakeOff();
        }

        private void TakeOff() {
            var planePosition = AutomatedObject.transform.position;
            if (Vector3.Distance(planePosition, AutomatedObject.TakeOffPoint.position) >= .5f) {
                Vector3 targetPoint = AutomatedObject.TakeOffPoint.position - planePosition;
                Vector3.MoveTowards(planePosition, targetPoint, AutomatedObject.TakeOffSpeed * Time.deltaTime);
            } else {
                Fulfil();
            }
        }
    }
}