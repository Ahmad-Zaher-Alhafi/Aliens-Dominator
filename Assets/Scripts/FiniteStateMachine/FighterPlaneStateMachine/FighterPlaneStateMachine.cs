using System;
using System.Collections.Generic;
using ScriptableObjects;
using SecurityWeapons;
using UnityEditor;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class FighterPlaneStateMachine : StateMachine<FighterPlaneState, FighterPlane, FighterPlaneStateType> {
        public override void Init(FighterPlane automatedObject, Enum initialState) {
            base.Init(automatedObject, initialState);
            GetState<DeactivatedState>().Fulfil();
            GetState<DestroyedState>().Fulfil();
        }

        protected override void CreateStates() {
            foreach (FighterPlaneStateMachineData.StateData stateData in StateMachineData.statesData) {
                FighterPlaneState state = stateData.originStateType switch {
                    FighterPlaneStateType.Deactivated => new DeactivatedState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.TakingOff => new TakingOffState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.Patrolling => new PatrollingState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.Aiming => new AimingState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.Shooting => new ShootingState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.GoingBackToBase => new GoingBackToBaseState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.GettingHit => new GettingHitState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    FighterPlaneStateType.Destroyed => new DestroyedState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<FighterPlaneState, FighterPlane, FighterPlaneStateType>>());
                States.Add(state.Type, state);
            }
        }

        public override void OnAutomationStatusChanged() {
            base.OnAutomationStatusChanged();
            if (AutomatedObject.IsAutomatingEnabled) {
                AutomatedObject.TakeOff();
            } else {
                AutomatedObject.GetBackToBase();
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(FighterPlaneStateMachine))]
        public class FighterPlaneStateMachineEditor : StateMachineEditor<FighterPlaneStateType> { }
#endif
    }
}