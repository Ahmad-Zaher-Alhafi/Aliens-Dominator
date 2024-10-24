﻿using System;
using System.Collections.Generic;
using ScriptableObjects;
using SecurityWeapons;
using UnityEditor;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public class FighterPlaneStateMachine : StateMachine<FighterPlaneState, FighterPlane, FighterPlaneStateType> {
        public override void Init(FighterPlane automatedObject, Enum initialState) {
            base.Init(automatedObject, initialState);
            GetState<DeactivatedState>().Fulfil();
        }

        protected override void CreateStates() {
            foreach (FighterPlaneStateMachineData.StateData stateData in StateMachineData.statesData) {
                FighterPlaneState state = stateData.originStateType switch {
                    FighterPlaneStateType.Deactivated => new DeactivatedState(AutomatedObject),
                    FighterPlaneStateType.TakingOff => new TakingOffState(AutomatedObject),
                    FighterPlaneStateType.Patrolling => new PatrollingState(AutomatedObject),
                    FighterPlaneStateType.Aiming => new AimingState(AutomatedObject),
                    FighterPlaneStateType.Shooting => new ShootingState(AutomatedObject),
                    FighterPlaneStateType.GoingBackToBase => new GoingBackToBaseState(AutomatedObject),
                    FighterPlaneStateType.Destroyed => new DestroyedState(AutomatedObject),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<FighterPlaneState, FighterPlane, FighterPlaneStateType>>());
                States.Add(state.Type, state);
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(FighterPlaneStateMachine))]
        public class FighterPlaneStateMachineEditor : StateMachineEditor<FighterPlaneStateType> { }
#endif
    }
}