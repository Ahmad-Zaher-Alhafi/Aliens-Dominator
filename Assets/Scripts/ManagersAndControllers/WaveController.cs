using System.Collections.Generic;
using System.Linq;
using Context;
using Multiplayer;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace ManagersAndControllers {
    public class WaveController : NetworkBehaviour {
        [Header("Points")]
        [SerializeField] private List<SpawnPoint> spawnPoints;
        [SerializeField] private List<TargetPoint> attackPoints;

        [Header("Waves Setup")]
        [SerializeField] private List<Wave> waves;

        [SerializeField] private Wave testWave;
        public Wave TestWave => testWave;

        [SerializeField] private Wave cinematicWave;
        public Wave CinematicWave => cinematicWave;

        [Header("Path drawer")]
        [SerializeField] private PathDrawer pathDrawerPrefab;
        [SerializeField] private Transform pathDrawersHolder;

        public int WavesCount => waves.Count;
        public Wave CurrentWave => CurrentWaveIndex < 0 ? null : waves[CurrentWaveIndex];

        public int CurrentWaveIndex => currentWaveIndex;
        private int currentWaveIndex = -1;

        public int NextWaveIndex => currentWaveIndex + 1;
        public bool HasWaveStarted { get; private set; }

        private readonly List<List<Vector3>> pathsToDraw = new();

        private void Awake() {
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork += UpdateWavesAirPathsTargetPoints;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) {
                InitWave(testWave);
            }
        }

        public void StartNextWave() {
            currentWaveIndex = NextWaveIndex;
            Wave wave = waves[currentWaveIndex];
            if (IsServer) {
                InitWave(wave);
            }
            DrawWavePaths();
            HasWaveStarted = true;
            Ctx.Deps.EventsManager.TriggerWaveStarted(wave);
        }

        private void InitWave(Wave wave) {
            for (int i = 0; i < wave.NumOfSpawnPoints; i++) {
                SpawnPoint randomSpawnPoint = MathUtils.GetRandomObjectFromList(spawnPoints);
                SpawnPointPath pathToFollow;
                TargetPoint targetPoint;

                for (int j = 0; j < wave.NumOfGroundPaths; j++) {
                    pathToFollow = MathUtils.GetRandomObjectFromList(randomSpawnPoint.GroundPaths);
                    targetPoint = MathUtils.GetRandomObjectFromList(attackPoints);
                    // Do not add it if already exists
                    if (wave.AddWavePath(randomSpawnPoint, pathToFollow, true, targetPoint)) continue;
                    AddPathToDraw(randomSpawnPoint.transform.position, pathToFollow.PathPoints.Select(point => point.transform.position).ToList(), targetPoint.transform.position);
                }

                for (int j = 0; j < wave.NumOfAirPaths; j++) {
                    pathToFollow = randomSpawnPoint.AirPath;
                    targetPoint = Ctx.Deps.ConstructionController.GetRandomDefenceWeapon()?.EnemyTargetPoint ?? Ctx.Deps.GameController.GetRandomPlayer().EnemyTargetPoint;
                    wave.AddWavePath(randomSpawnPoint, pathToFollow, false, targetPoint);
                }
            }
        }

        private void AddPathToDraw(Vector3 startPoint, List<Vector3> pathPoints, Vector3 endPoint) {
            pathPoints.Insert(0, startPoint);
            pathPoints.Add(endPoint);
            pathsToDraw.Add(pathPoints);
        }

        private void DrawWavePaths() {
            DrawPaths(pathsToDraw);
            pathsToDraw.Clear();
        }

        private void DrawPath(List<Vector3> pathPoints) {
            PathDrawer pathDrawer = pathDrawerPrefab.GetObject<PathDrawer>(pathDrawersHolder);
            pathDrawer.Draw(pathPoints);
        }

        private void DrawPaths(List<List<Vector3>> pathsPoints) {
            if (!IsServer) return;

            SerializedNetworkVector3List[] clientPathsPoints = new SerializedNetworkVector3List[pathsPoints.Count];

            for (int i = 0; i < pathsPoints.Count; i++) {
                clientPathsPoints[i] = new SerializedNetworkVector3List(pathsPoints[i].ToArray());
            }

            DrawPathsClientRPC(clientPathsPoints);
        }

        [ClientRpc]
        private void DrawPathsClientRPC(SerializedNetworkVector3List[] pathsPoints) {
            foreach (SerializedNetworkVector3List pathPoints in pathsPoints) {
                DrawPath(pathPoints.Objects.ToList());
            }
        }

        /// <summary>
        /// New joined player should be considered as a target point by air creatures
        /// </summary>
        private void UpdateWavesAirPathsTargetPoints(Player.Player player) {
            foreach (Wave wave in waves) {
                wave.ReassignAirTargetPoints();
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork -= UpdateWavesAirPathsTargetPoints;
        }
    }
}