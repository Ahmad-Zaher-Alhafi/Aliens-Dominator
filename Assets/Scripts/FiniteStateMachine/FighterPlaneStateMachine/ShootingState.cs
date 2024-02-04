using Projectiles;
using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class ShootingState : FighterPlaneState {
        public override FighterPlaneStateType Type => FighterPlaneStateType.Shooting;
        public override bool CanBeActivated() => AutomatedObject.WeaponSensor.TargetToAimAt != null && Time.time >= lastTimeShot + FireRate + AutomatedObject.CoolDownTime;

        private float FireRate => 1f / (AutomatedObject.HasToUseRockets ? AutomatedObject.RocketsPerSecond : AutomatedObject.BulletsPerSecond);
        private float lastTimeShot;

        public ShootingState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            lastTimeShot = Time.time;
            AutomatedObject.Shoot(AutomatedObject.HasToUseRockets ? typeof(Rocket) : typeof(Bullet), AutomatedObject.WeaponSensor.TargetToAimAt);
            Fulfil();
        }
    }
}