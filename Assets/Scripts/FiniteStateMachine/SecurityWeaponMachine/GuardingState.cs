using SecurityWeapons;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public class GuardingState<TEnemyType> : SecurityWeaponState<TEnemyType> where TEnemyType : IAutomatable {
        public override SecurityWeaponStateType Type => SecurityWeaponStateType.Guarding;
        public override bool CanBeActivated() => AutomatedObject.WeaponSensor.TargetToAimAt == null;

        private GuardingComponent guardingComponent;

        public GuardingState(SecurityWeapon<TEnemyType> automatedObject, bool checkWhenAutomatingDisabled) : base(automatedObject, checkWhenAutomatingDisabled) { }


        public override void Tick() {
            base.Tick();
            Guard();
        }

        // To let the weapon rotates randomly (escorting area)
        private void Guard() {
            if (guardingComponent == null) {
                guardingComponent = AutomatedObject.GetComponent<GuardingComponent>();
                guardingComponent.Init(AutomatedObject.InitialRotation, AutomatedObject.RotateOnXAxisRange, AutomatedObject.RotateOnYAxisRange, OnGuardingFinishCallBack);
            }

            guardingComponent.Guard();
        }

        private void OnGuardingFinishCallBack() {
            Fulfil();
        }
    }
}