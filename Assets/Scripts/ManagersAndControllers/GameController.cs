using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using SecurityWeapons;
using UI;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace ManagersAndControllers {
    public class GameController : NetworkBehaviour {
        public enum ViewMode {
            None,
            General,
            FPS,
            TopDown
        }

        [Header("Nav Mesh")]
        [SerializeField] private List<NavMeshSurface> navMeshSurfaces;
        [SerializeField] private int winParticlesLoopCount;
        [SerializeField] private List<ParticleSystem> winParticles;
        [SerializeField] private float winParticlesRepeatTime;
        [SerializeField] private AnimatedText animatedTextPrefab;

        public ViewMode CurrentViewMode { get; private set; }

        private readonly List<Player.Player> players = new();

        public Player.Player Player { get; private set; }

        private void Awake() {
            foreach (NavMeshSurface navMeshSurface in navMeshSurfaces) {
                navMeshSurface.BuildNavMesh();
            }

            SwitchViewModeTo(ViewMode.General);

            Ctx.Deps.EventsManager.EnemyGotHit += OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished += OnWaveFinished;
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork += OnPlayerSpawnedOnNetwork;
            Ctx.Deps.EventsManager.PlayerDespawnedFromNetwork += OnPlayerDespawnedFromNetwork;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            SwitchViewModeTo(ViewMode.General);
        }

        private void Update() {
            if (Ctx.Deps.InputActions.FPSAndTopDownViewsSharedActions.TopDownViewAction.WasPressedThisFrame()) {
                SwitchViewModeTo(ViewMode.TopDown);
            }

            if (Ctx.Deps.InputActions.FPSAndTopDownViewsSharedActions.FPSViewAction.WasPressedThisFrame()) {
                SwitchViewModeTo(ViewMode.FPS);
            }
        }

        private void SwitchViewModeTo(ViewMode viewModeToSwitchTo) {
            if (viewModeToSwitchTo == CurrentViewMode) return;

            switch (viewModeToSwitchTo) {
                case ViewMode.General:
                    Ctx.Deps.InputActions.FPSViewActions.Disable();
                    Ctx.Deps.InputActions.TopDownViewActions.Disable();
                    Ctx.Deps.InputActions.FPSAndTopDownViewsSharedActions.Disable();
                    Ctx.Deps.CameraController.SwitchToGeneralCamera();
                    break;
                case ViewMode.FPS:
                    Ctx.Deps.InputActions.TopDownViewActions.Disable();
                    Ctx.Deps.InputActions.FPSViewActions.Enable();
                    Ctx.Deps.InputActions.FPSAndTopDownViewsSharedActions.Enable();
                    Ctx.Deps.CameraController.SwitchToPlayerCamera();
                    break;
                case ViewMode.TopDown:
                    Ctx.Deps.InputActions.FPSViewActions.Disable();
                    Ctx.Deps.InputActions.TopDownViewActions.Enable();
                    Ctx.Deps.InputActions.FPSAndTopDownViewsSharedActions.Enable();
                    Ctx.Deps.CameraController.SwitchToTopDownCamera();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewModeToSwitchTo), viewModeToSwitchTo, null);
            }

            StartCoroutine(SetCurrentModeDelayed(viewModeToSwitchTo));
        }

        /// <summary>
        /// Switching view mode at the end of the frame to give time for the CameraBrain to start blinding,
        /// As the CameraBrain's IsBlinding property is used to show and hide some UI during cameras switch, and it needs time to be set to ture
        /// </summary>
        /// <param name="viewMode"></param>
        /// <returns></returns>
        private IEnumerator SetCurrentModeDelayed(ViewMode viewMode) {
            yield return new WaitForEndOfFrame();
            ViewMode previousViewMode = CurrentViewMode;
            CurrentViewMode = viewMode;
            Ctx.Deps.EventsManager.TriggerViewModeChanged(previousViewMode, CurrentViewMode);
        }

        private void OnPlayerSpawnedOnNetwork(Player.Player player) {
            if (player.IsOwner) {
                Player = player;
                SwitchViewModeTo(ViewMode.FPS);
            }
            players.Add(player);
        }

        private void OnPlayerDespawnedFromNetwork(Player.Player player) {
            if (player.IsOwner) {
                Player = null;
                QuitMatch();
            }
            players.Remove(player);
        }

        public Player.Player GetPlayerOfClientId(ulong playerClientId) {
            return players.SingleOrDefault(player => player.OwnerClientId == playerClientId);
        }

        private void OnEnemyGotHit(Creature creature) {
            if (Ctx.Deps.WaveController.HasWaveStarted) return;
            StartNextWave();
        }

        public void ShowAnimatedText(string text, Vector3 createPosition, Color color) {
            animatedTextPrefab.GetObject<AnimatedText>(null).ShowText(text, createPosition, color);
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
            SwitchViewModeTo(ViewMode.General);
            Ctx.Deps.Matchmaker.QuitMatch();
        }

        public override void OnDestroy() {
            Ctx.Deps.EventsManager.EnemyDied -= OnEnemyGotHit;
            Ctx.Deps.EventsManager.WaveFinished -= OnWaveFinished;
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork -= OnPlayerSpawnedOnNetwork;
            Ctx.Deps.EventsManager.PlayerDespawnedFromNetwork -= OnPlayerDespawnedFromNetwork;
        }


#if UNITY_EDITOR
        [Space, Header("Editor Stuff")]
        [SerializeField, HideInInspector] private bool activateAllWeaponsOnStart;

        [CustomEditor(typeof(GameController))]
        public class GameControllerEditor : Editor {
            private SerializedProperty activateAllWeaponsOnStart;
            private int timeScale = 1;

            private void OnEnable() {
                activateAllWeaponsOnStart = serializedObject.FindProperty("activateAllWeaponsOnStart");
                if (!Application.isPlaying) return;

                Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork += PlayerSpawnedOnNetwork;
            }

            private void OnDisable() {
                if (!Application.isPlaying) return;
                Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork -= PlayerSpawnedOnNetwork;
            }

            private void PlayerSpawnedOnNetwork(Player.Player player) {
                Ctx.Deps.GameController.StartCoroutine(UpdateWeaponCommandersActivationDelayed());
            }

            private IEnumerator UpdateWeaponCommandersActivationDelayed() {
                yield return new WaitForEndOfFrame();
                GameController gameController = (GameController) target;
                if (!gameController.IsServer) yield return null;

                foreach (AutomationCommander automationCommander in FindObjectsOfType<AutomationCommander>()) {
                    automationCommander.SetAutomationStatusServerRPC(activateAllWeaponsOnStart.boolValue);
                }
            }

            private void OnSceneGUI() {
                serializedObject.Update();

                EditorGUILayout.PropertyField(activateAllWeaponsOnStart, new GUIContent("Activate All Weapons On Start"));
                serializedObject.ApplyModifiedProperties();
            }

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                serializedObject.Update();

                EditorGUILayout.PropertyField(activateAllWeaponsOnStart, new GUIContent("Activate All Weapons On Start"));
                serializedObject.ApplyModifiedProperties();

                if (!Application.isPlaying) {
                    EditorGUILayout.HelpBox("Editor content is shown only in play mode", MessageType.Info);
                    return;
                }

                GameController gameController = (GameController) target;

                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Start waves")) {
                    Ctx.Deps.EventsManager.TriggerWaveFinished();
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Win the game")) {
                    gameController.GameOver(true);
                }


                timeScale = EditorGUILayout.IntField("Time scale:", timeScale);

                GUI.backgroundColor = Color.gray;
                if (GUILayout.Button("Set time scale")) {
                    Time.timeScale = timeScale;
                }
            }
        }
#endif
    }
}