using System.Collections.Generic;
using System.Linq;
using Context;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace ManagersAndControllers {
    public class WaveController : MonoBehaviour {
        [Header("Points")]
        [SerializeField] private List<SpawnPoint> spawnPoints;
        [SerializeField] private List<TargetPoint> attackPoints;

        [Header("Waves Setup")]
        [SerializeField] private List<Wave> waves;
        
        [SerializeField] private Wave cinematicWave;
        public Wave CinematicWave => cinematicWave;

        public int WavesCount => waves.Count;
        public Wave CurrentWave => waves[Ctx.Deps.GameController.CurrentWaveIndex];

        private readonly List<List<Vector3>> pathsToDraw = new();

        public void StartWave(int waveIndex) {
            Wave wave = waves[waveIndex];
            InitWave(wave);
            DrawWavePaths();
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
                    // Do not draw it if already exists
                    if (wave.AddWavePath(randomSpawnPoint, pathToFollow, true, targetPoint)) continue;
                    AddPathToDraw(randomSpawnPoint.transform.position, pathToFollow.PathPoints.Select(point => point.transform.position).ToList(), targetPoint.transform.position);
                }

                for (int j = 0; j < wave.NumOfAirPaths; j++) {
                    pathToFollow = randomSpawnPoint.AirPath;
                    targetPoint = MathUtils.GetRandomObjectFromList(NetworkManager.Singleton.ConnectedClients.Values.ToList()).PlayerObject.GetComponent<Player.Player>().EnemyTargetPoint;
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
            Ctx.Deps.GameController.DrawPaths(pathsToDraw);
            pathsToDraw.Clear();
        }
    }
}