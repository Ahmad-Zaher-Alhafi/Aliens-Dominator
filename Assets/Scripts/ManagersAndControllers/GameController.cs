﻿using System;
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
            SwitchViewModeTo(ViewMode.FPS);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            QuitMatch();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                SwitchViewModeTo(ViewMode.TopDown);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SwitchViewModeTo(ViewMode.FPS);
            }
        }

        private void SwitchViewModeTo(ViewMode viewMode) {
            if (viewMode == CurrentViewMode) return;

            switch (viewMode) {
                case ViewMode.General:
                    Ctx.Deps.CameraController.SwitchToGeneralCamera();
                    break;
                case ViewMode.FPS:
                    Ctx.Deps.CameraController.SwitchToPlayerCamera();
                    break;
                case ViewMode.TopDown:
                    Ctx.Deps.CameraController.SwitchToTopDownCamera();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewMode), viewMode, null);
            }

            StartCoroutine(SetCurrentModeDelayed(viewMode));
        }

        /// <summary>
        /// Switching view mode at the end of the frame to give time for the CameraBrain to start blinding,
        /// As the CameraBrain's IsBlinding property is used to show and hide some UI during cameras switch, and it needs time to be set to ture
        /// </summary>
        /// <param name="viewMode"></param>
        /// <returns></returns>
        private IEnumerator SetCurrentModeDelayed(ViewMode viewMode) {
            yield return new WaitForEndOfFrame();
            CurrentViewMode = viewMode;
        }

        private void OnPlayerSpawnedOnNetwork(Player.Player player) {
            if (player.IsOwner) {
                Player = player;
            }
            players.Add(player);
        }
        private void OnPlayerDespawnedFromNetwork(Player.Player player) {
            if (player.IsOwner) {
                Player = null;
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