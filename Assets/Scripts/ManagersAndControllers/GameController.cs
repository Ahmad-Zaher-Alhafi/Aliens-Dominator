using System.Collections;
using System.Collections.Generic;
using Context;
using Creatures;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace ManagersAndControllers {
    public class GameController : NetworkBehaviour {
        [Header("Nav Mesh")]
        [SerializeField] private List<NavMeshSurface> navMeshSurfaces;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            Cursor.lockState = CursorLockMode.None;
        }

        private void Awake() {
            foreach (NavMeshSurface navMeshSurface in navMeshSurfaces) {
                navMeshSurface.BuildNavMesh();
            }

            Ctx.Deps.EventsManager.EnemyGotHit += OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished += OnWaveFinished;
        }

        private void Update() {
#if UNITY_EDITOR
            if (EventSystem.current.currentSelectedGameObject != null) return;

            if (Input.GetMouseButtonDown(1)) {
                Cursor.lockState = CursorLockMode.None;
            } else if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
            }
#endif
        }

        private void OnEnemyGotHit(Creature creature) {
            if (Ctx.Deps.WaveController.HasWaveStarted) return;
            StartNextWave();
        }

        public new Coroutine StartCoroutine(IEnumerator routine) {
            return base.StartCoroutine(routine);
        }

        private void StartNextWave() {
            StartCoroutine(StartNextWaveDelayed());
        }

        private void OnWaveFinished() {
            Debug.Log($"Wave {Ctx.Deps.WaveController.CurrentWaveIndex} has been finished");

            if (Ctx.Deps.WaveController.NextWaveIndex >= Ctx.Deps.WaveController.WavesCount) {
                Debug.Log("Game over, You win");
                return;
            }

            StartNextWave();
        }

        private IEnumerator StartNextWaveDelayed() {
            Assert.IsTrue(Ctx.Deps.WaveController.NextWaveIndex < Ctx.Deps.WaveController.WavesCount, "No next wave found");

            yield return new WaitForEndOfFrame();

            Ctx.Deps.WaveController.StartNextWave();
            Debug.Log($"Wave {Ctx.Deps.WaveController.CurrentWaveIndex} has been started");
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

                if (!Application.isPlaying) {
                    EditorGUILayout.HelpBox("Editor content is shown only in play mode", MessageType.Info);
                    return;
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Start waves")) {
                    Ctx.Deps.EventsManager.TriggerWaveFinished();
                }
            }
        }
#endif
    }
}