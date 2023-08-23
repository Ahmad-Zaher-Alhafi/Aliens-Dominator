using System.Collections;
using System.Collections.Generic;
using Context;
using Creatures;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace ManagersAndControllers {
    public class GameController : MonoBehaviour {
        [SerializeField] private List<NavMeshSurface> navMeshSurfaces;

        private int currentWaveIndex = -1;
        public bool HasWaveStarted { get; private set; }
        int NextWaveIndex => currentWaveIndex + 1;

        private void Awake() {
            foreach (NavMeshSurface navMeshSurface in navMeshSurfaces) {
                navMeshSurface.BuildNavMesh();
            }

            Ctx.Deps.EventsManager.EnemyGotHit += OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished += OnWaveFinished;
        }

        private void OnEnemyGotHit(Creature creature) {
            if (HasWaveStarted) return;
            StartNextWave();
        }

        public new Coroutine StartCoroutine(IEnumerator routine) {
            return base.StartCoroutine(routine);
        }

        private void StartNextWave() {
            Assert.IsTrue(NextWaveIndex < Ctx.Deps.CreatureSpawnController.WavesCount, "No next wave found");

            HasWaveStarted = true;
            Ctx.Deps.EventsManager.TriggerWaveStarted(NextWaveIndex);
            currentWaveIndex = NextWaveIndex;
            Debug.Log($"Wave {currentWaveIndex} has been started");
        }

        private void OnWaveFinished() {
            Debug.Log($"Wave {currentWaveIndex} has been finished");

            if (NextWaveIndex >= Ctx.Deps.CreatureSpawnController.WavesCount) {
                Debug.Log($"Game over, You win");
                return;
            }

            StartNextWave();
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.EnemyDied -= OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished -= OnWaveFinished;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GameController))]
        public class GameControllerEditor : Editor {
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                GameController gameController = (GameController) target;

                if (GUILayout.Button("Start waves")) {
                    if (Application.isPlaying) {
                        gameController.StartNextWave();
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}