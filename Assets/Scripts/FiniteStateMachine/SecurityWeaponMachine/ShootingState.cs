using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class ShootingState : SecurityWeaponState {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Shooting;
        public override bool CanBeActivated() => StateObject.WeaponSensor.TargetToAimAt != null && Time.time >= lastTimeShot + FireRate;

        private float FireRate => 1 / StateObject.BulletsPerSecond;
        private float lastTimeShot;

        public ShootingState(SecurityWeapon securityWeapon) : base(securityWeapon) { }


        public override void Activate() {
            base.Activate();
            lastTimeShot = Time.time;
            StateObject.Shoot(StateObject.WeaponSensor.TargetToAimAt);
            Fulfil();
        }
    }
}