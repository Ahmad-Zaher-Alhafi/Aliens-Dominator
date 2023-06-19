using System;
using System.Collections.Generic;
using System.Linq;
using Creatures;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiniteStateMachine.CreatureStateMachine {
    public class CreatureStateMachine : StateMachine<CreatureState, Creature> {
        [SerializeField] private CreatureStateMachineData creatureStateMachineData;
        public override CreatureState CurrentState { get; protected set; }

        public override void Init(Creature objectToAutomate, Enum initialState) {
            base.Init(objectToAutomate, initialState);

            if (CurrentState.Type is CreatureStateType.None or CreatureStateType.Dead) {
                CurrentState.Fulfil();
            }
        }

        protected override void CreateStates() {
            foreach (CreatureStateMachineData.StateData stateData in creatureStateMachineData.statesData) {
                CreatureState state = stateData.originStateType switch {
                    CreatureStateType.None => new NoneState(ObjectToAutomate),
                    CreatureStateType.Idle => new IdleState(ObjectToAutomate),
                    CreatureStateType.Patrolling => new PatrollingState(ObjectToAutomate),
                    CreatureStateType.FollowingPath => new FollowingPathState(ObjectToAutomate),
                    CreatureStateType.ChasingTarget => new ChasingTargetState(ObjectToAutomate),
                    CreatureStateType.GettingHit => new GettingHitState(ObjectToAutomate),
                    CreatureStateType.Attacking => new AttackingState(ObjectToAutomate),
                    CreatureStateType.RunningAway => new RunningAwayState(ObjectToAutomate),
                    CreatureStateType.Dead => new DeadState(ObjectToAutomate),
                    _ => throw new ArgumentOutOfRangeException()
                };

                StatesTransitions.Add(state, new List<Transition<CreatureState, Creature>>());
                States.Add(state.Type, state);
            }
        }

        protected override void LinkStatesWithTransitions() {
            foreach (CreatureStateMachineData.StateData stateData in creatureStateMachineData.statesData) {
                foreach (CreatureStateMachineData.StateData.TransitionData transitionData in stateData.transitionsData) {
                    CreatureState originState = States[stateData.originStateType];
                    CreatureState destinationState = States[transitionData.destinationStateType];
                    StatesTransitions[originState].Add(new Transition<CreatureState, Creature>(originState, destinationState, transitionData.canInterrupts));
                }
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
            CreatureStateType randomCreatureState = randomNumber switch {
                0 => CreatureStateType.Idle,
                1 => CreatureStateType.Patrolling,
                _ => throw new ArgumentOutOfRangeException()
            };

            StatesTransitions.Keys.Single(pair => pair.Type == randomCreatureState).IsNextCinematicState = true;
        }

        private void ForceState(CreatureStateType creatureStateType) {
            CurrentState.Interrupt();
            CurrentState = States[creatureStateType];
            CurrentState.Activate();
        }
        
#if UNITY_EDITOR
        [CustomEditor(typeof(CreatureStateMachine))]
        public class CreatureStateEditor : Editor {
            [SerializeField] private CreatureStateType creatureStateType;
            [SerializeField] private bool overrideCurrentState;

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                CreatureStateMachine creatureStateMachine = (CreatureStateMachine) target;

                // Show current state
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current State");
                GUI.enabled = false;
                EditorGUILayout.TextField(creatureStateMachine.CurrentState?.Type.ToString() ?? CreatureStateType.None.ToString());
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                overrideCurrentState = EditorGUILayout.Toggle("Override Current State", overrideCurrentState);
                EditorGUILayout.EndHorizontal();

                if (!overrideCurrentState) return;

                EditorGUILayout.BeginHorizontal();
                creatureStateType = (CreatureStateType) EditorGUILayout.EnumPopup("State to force", creatureStateType);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Force state")) {
                    if (Application.isPlaying) {
                        creatureStateMachine.ForceState(creatureStateType);
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}