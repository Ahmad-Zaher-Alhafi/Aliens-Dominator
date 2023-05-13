using System;
using System.Collections.Generic;
using System.Linq;
using Creatures;
using FiniteStateMachine.States;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.CreatureMachine {
    public class CreatureStateMachine : StateMachine<Creature> {
        protected override void Awake() {
            StatesHolder = GameObject.FindGameObjectWithTag("CreatureStateMachine").transform;
            base.Awake();
        }

        public override void Init(Creature creature, StateType initialState) {
            base.Init(creature, initialState);

            if (CurrentState.Type is StateType.None or StateType.Dead) {
                CurrentState.Fulfil();
            }
        }

        protected override void CreateStates() {
            foreach (StateVisualiser stateVisualiser in StateVisualisers) {
                State state = stateVisualiser.StateType switch {
                    StateType.None => new NoneState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.Idle => new IdleState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.Patrolling => new PatrollingState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.FollowingPath => new FollowingPathState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.ChasingTarget => new ChasingTargetState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.GettingHit => new GettingHitState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.Attacking => new AttackingState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.RunningAway => new RunningAwayState(stateVisualiser.IsFinal, ObjectToAutomate),
                    StateType.Dead => new DeadState(stateVisualiser.IsFinal, ObjectToAutomate),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition>());
                States.Add(state.Type, state);
            }
        }

        protected override void Tick() {
            if (ObjectToAutomate.IsDead) return;
            base.Tick();
        }

        /// <summary>
        /// Some states have a random chance activation, this function will decide randomly the next state that could be activated
        /// </summary>
        public void SetNextCinematicState() {
            int randomNumber = Random.Range(0, 2);
            StateType randomState = randomNumber switch {
                0 => StateType.Idle,
                1 => StateType.Patrolling,
                _ => throw new ArgumentOutOfRangeException()
            };

            StatesTransitions.Keys.Single(pair => pair.Type == randomState).IsNextCinematicState = true;
        }
    }
}