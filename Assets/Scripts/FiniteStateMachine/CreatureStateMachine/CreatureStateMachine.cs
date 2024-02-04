using System;
using System.Collections.Generic;
using System.Linq;
using Creatures;
using ScriptableObjects;
using UnityEditor;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.CreatureStateMachine {
    public class CreatureStateMachine : StateMachine<CreatureState, Creature, CreatureStateType> {
        public override void Init(Creature automatedObject, Enum initialState) {
            base.Init(automatedObject, initialState);
            GetState<NoneState>().Fulfil();
            GetState<DeadState>().Fulfil();
        }

        protected override void CreateStates() {
            foreach (CreatureStateMachineData.StateData stateData in StateMachineData.statesData) {
                CreatureState state = stateData.originStateType switch {
                    CreatureStateType.None => new NoneState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.Idle => new IdleState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.Patrolling => new PatrollingState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.FollowingPath => new FollowingPathState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.ChasingTarget => new ChasingTargetState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.GettingHit => new GettingHitState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.Attacking => new AttackingState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.RunningAway => new RunningAwayState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.Dead => new DeadState(AutomatedObject, true),
                    CreatureStateType.SpecialAbility => new SpecialAbilityState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    CreatureStateType.Spawning => new SpawningState(AutomatedObject, stateData.checkedWhenAutomationDisabled),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<CreatureState, Creature, CreatureStateType>>());
                States.Add(state.Type, state);
            }
        }

        /// <summary>
        /// Some states have a random chance activation, this function will decide randomly the next state that could be activated
        /// </summary>
        public void SetNextCinematicState() {
            int randomNumber = Random.Range(0, 2);
            CreatureStateType randomCreatureState = randomNumber switch {
                0 => CreatureStateType.Idle,
                1 => CreatureStateType.Patrolling,
                _ => throw new ArgumentOutOfRangeException()
            };

            StatesTransitions.Keys.Single(pair => pair.Type == randomCreatureState).IsNextCinematicState = true;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(CreatureStateMachine))]
        public class CreatureStateMachineEditor : StateMachineEditor<CreatureStateType> { }
#endif
    }
}