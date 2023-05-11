using System;
using System.Collections.Generic;
using System.Linq;
using Creatures;
using FiniteStateMachine.States;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine {
    public class StateMachine : MonoBehaviour {
        public State CurrentState { get; private set; }

        private Transform statesHolder;

        private readonly Dictionary<State, List<Transition>> statesTransitions = new();

        private List<TransitionVisualiser> transitionVisualisers;
        private List<Transition> currentStatePossibleTransitions;
        private readonly Dictionary<StateType, State> states = new();
        private Creature creature;
        private bool isInitialized;

        public void Init(Creature creature, StateType initialState) {
            if (!isInitialized) {
                isInitialized = true;

                this.creature = creature;
                transitionVisualisers ??= FindObjectsOfType<TransitionVisualiser>().ToList();
                statesHolder ??= GameObject.FindWithTag("StatesHolder").transform;

                if (transitionVisualisers.Count == 0) return;

                // Get all states
                foreach (Transform stateVisualizerObject in statesHolder) {
                    StateVisualiser stateVisualiser = stateVisualizerObject.GetComponent<StateVisualiser>();
                    switch (stateVisualiser.StateType) {
                        case StateType.None:
                            statesTransitions.Add(new NoneState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.Idle:
                            statesTransitions.Add(new IdleState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.Patrolling:
                            statesTransitions.Add(new PatrollingState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.FollowingPath:
                            statesTransitions.Add(new FollowingPathState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.ChasingTarget:
                            statesTransitions.Add(new ChasingTargetState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.GettingHit:
                            statesTransitions.Add(new GettingHitState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.Attacking:
                            statesTransitions.Add(new AttackingState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.RunningAway:
                            statesTransitions.Add(new RunningAwayState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        case StateType.Dead:
                            statesTransitions.Add(new DeadState(stateVisualiser.IsFinal, creature), new List<Transition>());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                foreach (State state in statesTransitions.Keys) {
                    states.Add(state.Type, state);
                }

                // Get all transitions
                foreach (TransitionVisualiser transitionVisualiser in transitionVisualisers) {
                    State originState = statesTransitions.Keys.Single(stat => stat.Type == transitionVisualiser.OriginStateType);
                    State destinationState = statesTransitions.Keys.Single(stat => stat.Type == transitionVisualiser.DestinationStateType);
                    statesTransitions[originState].Add(new Transition(transitionVisualiser, originState, destinationState));
                }
            }


            CurrentState = states[initialState];
            CurrentState.Activate();
            if (CurrentState.Type is StateType.None or StateType.Dead) {
                FulfillCurrentState();
            }
        }

        private void Update() => Tick();

        private void Tick() {
            if (creature.IsDead) return;

            if (CurrentState.IsActive) {
                CurrentState.Tick();
            }

            currentStatePossibleTransitions = statesTransitions[CurrentState].FindAll(transition => transition.IsTransitionPossible());

            // Find any transitions that can interrupt the current state
            foreach (var transition in currentStatePossibleTransitions.Where(transition => transition.CanInterrupts)) {
                ActivateDestinationState(transition);
                return;
            }

            if (CurrentState.IsActive) return;
            // Find the next state when the currentState is not active anymore
            foreach (var transition in currentStatePossibleTransitions) {
                ActivateDestinationState(transition);
                return;
            }
        }

        private void ActivateDestinationState(Transition transition) {
            if (transition.CanInterrupts) {
                CurrentState.Interrupt();
            }

            CurrentState = transition.DestinationState;
            CurrentState.Activate();
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

            statesTransitions.Keys.Single(pair => pair.Type == randomState).IsNextCinematicState = true;
        }

        public void FulfillCurrentState() {
            if (CurrentState.IsActive) {
                CurrentState.Fulfil();
            }
        }

        public T GetState<T>() where T : State {
            return states.Values.OfType<T>().Single();
        }
    }
}