using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace ManagersAndControllers {
    public class GameController : NetworkBehaviour {
        [Header("Nav Mesh")]
        [SerializeField] private List<NavMeshSurface> navMeshSurfaces;
        [SerializeField] private int winParticlesLoopCount;
        [SerializeField] private List<ParticleSystem> winParticles;
        [SerializeField] private float winParticlesRepeatTime;

        public Player.Player Player { get; private set; }

        private void Awake() {
            foreach (NavMeshSurface navMeshSurface in navMeshSurfaces) {
                navMeshSurface.BuildNavMesh();
            }

            Ctx.Deps.EventsManager.EnemyGotHit += OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished += OnWaveFinished;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            Player = FindObjectsOfType<Player.Player>().Single(o => o.GetComponent<NetworkObject>().OwnerClientId == NetworkObject.OwnerClientId);
            Ctx.Deps.EventsManager.TriggerSpawnedOnNetwork();
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
                GameOver(true);
                return;
            }

            StartNextWave();
        }

        public void GameOver(bool hasWon) {
            if (hasWon) {
                Debug.Log("Game over, you win !");
                StartCoroutine(PlayWinParticlesDelayed(winParticlesLoopCount));
            } else {
                Debug.LogWarning("Game Over, you lose !");
            }
        }

        private IEnumerator StartNextWaveDelayed() {
            Assert.IsTrue(Ctx.Deps.WaveController.NextWaveIndex < Ctx.Deps.WaveController.WavesCount, "No next wave found");

            yield return new WaitForEndOfFrame();

            Ctx.Deps.WaveController.StartNextWave();
            Debug.Log($"Wave {Ctx.Deps.WaveController.CurrentWaveIndex} has been started");
        }

        private IEnumerator PlayWinParticlesDelayed(int loopsCount) {
            for (int i = 0; i < loopsCount; i++) {
                foreach (ParticleSystem winParticle in winParticles) {
                    winParticle.Play();
                }

                yield return new WaitForSeconds(winParticlesRepeatTime);
            }
        }

        public void QuitMatch() {
            StopAllCoroutines();
            Ctx.Deps.CameraController.SwitchToGeneralCamera();
            Ctx.Deps.Matchmaker.QuitMatch();
        }

        public override void OnDestroy() {
            Ctx.Deps.EventsManager.EnemyDied -= OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished -= OnWaveFinished;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GameController))]
        public class GameControllerEditor : Editor {
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                GameController gameController = (GameController) target;

                if (!Application.isPlaying) {
                    EditorGUILayout.HelpBox("Editor content is shown only in play mode", MessageType.Info);
                    return;
                }

                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Start waves")) {
                    Ctx.Deps.EventsManager.TriggerWaveFinished();
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Win the game")) {
                    gameController.GameOver(true);
                }
            }
        }
#endif
    }
}