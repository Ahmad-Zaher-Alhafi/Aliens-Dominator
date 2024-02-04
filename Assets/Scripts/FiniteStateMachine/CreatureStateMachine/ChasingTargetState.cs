using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class ChasingTargetState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.ChasingTarget;
        public override bool CanBeActivated() => (AutomatedObject.IsPoisoned || !AutomatedObject.HasToFollowPath) && AutomatedObject.ObjectToAttack != null;
        public override float? Speed => Vector3.Distance(AutomatedObject.transform.position, AutomatedObject.ObjectToAttack.transform.position) > 1
            ? AutomatedObject.RunSpeed
            : 0;
        protected override bool WaitForMoverToFulfill => true;
        protected override bool WaitForAnimatorToFulfill => false;

        public ChasingTargetState(Creature creature, bool checkWhenAutomatingDisabled) : base(creature, checkWhenAutomatingDisabled) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            AutomatedObject.Mover.ChaseTarget(OnMoverOrderFulfilled, AutomatedObject.ObjectToAttack.transform);
        }

        public override void Fulfil() {
            base.Fulfil();
            AutomatedObject.TargetReached = true;
        }
    }
}