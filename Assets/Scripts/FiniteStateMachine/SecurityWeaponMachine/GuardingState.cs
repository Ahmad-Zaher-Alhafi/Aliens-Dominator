using SecurityWeapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GuardingState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Guarding;
        public override bool CanBeActivated() => AutomatedObject.WeaponSensor.TargetToAimAt == null;

        public GuardingState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }
    }
}