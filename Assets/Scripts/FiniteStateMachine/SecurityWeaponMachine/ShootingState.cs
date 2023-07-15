using SecurityWeapons;
using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class ShootingState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Shooting;
        public override bool CanBeActivated() => AutomatedObject.WeaponSensor.TargetToAimAt != null && Time.time >= lastTimeShot + FireRate;

        private float FireRate => 1 / AutomatedObject.BulletsPerSecond;
        private float lastTimeShot;

        public ShootingState(SecurityWeapon<TEnemyType> securityWeapon) : base(securityWeapon) { }


        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            lastTimeShot = Time.time;
            AutomatedObject.Shoot(AutomatedObject.WeaponSensor.TargetToAimAt);
            Fulfil();
        }
    }
}