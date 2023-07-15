using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class ShootingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Shooting;
        public override bool CanBeActivated() => AutomatedObject.WeaponSensor.TargetToAimAt != null && Time.time >= lastTimeShot + FireRate;

        private float FireRate => 1f / AutomatedObject.BulletsPerSecond;
        private float lastTimeShot;

        public ShootingState(FighterPlane fighterPlane) : base(fighterPlane) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            lastTimeShot = Time.time;
            AutomatedObject.Shoot(AutomatedObject.WeaponSensor.TargetToAimAt);
            Fulfil();
        }
    }
}