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
                    CreatureStateType.None => new NoneState(AutomatedObject),
                    CreatureStateType.Idle => new IdleState(AutomatedObject),
                    CreatureStateType.Patrolling => new PatrollingState(AutomatedObject),
                    CreatureStateType.FollowingPath => new FollowingPathState(AutomatedObject),
                    CreatureStateType.ChasingTarget => new ChasingTargetState(AutomatedObject),
                    CreatureStateType.GettingHit => new GettingHitState(AutomatedObject),
                    CreatureStateType.Attacking => new AttackingState(AutomatedObject),
                    CreatureStateType.RunningAway => new RunningAwayState(AutomatedObject),
                    CreatureStateType.Dead => new DeadState(AutomatedObject),
                    CreatureStateType.SpecialAbility => new SpecialAbilityState(AutomatedObject),
                    CreatureStateType.Spawning => new SpawningState(AutomatedObject),
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