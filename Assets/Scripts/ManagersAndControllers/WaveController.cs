using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Multiplayer;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace ManagersAndControllers {
    public class WaveController : NetworkBehaviour {
        [Header("Points")]
        [SerializeField] private List<SpawnPoint> spawnPoints;
        [SerializeField] private List<TargetPoint> attackPoints;

        [Header("Waves Setup")]
        [SerializeField] private List<Wave> waves;

        [SerializeField] private Wave cinematicWave;
        public Wave CinematicWave => cinematicWave;

        [Header("Path drawer")]
        [SerializeField] private PathDrawer pathDrawerPrefab;
        [SerializeField] private Transform pathDrawersHolder;
        [SerializeField] private float fragmentationDistance = 3;
        [SerializeField] private float pathCreationSpeed = .03f;

        public int WavesCount => waves.Count;
        public Wave CurrentWave => waves[CurrentWaveIndex];
        
        public int CurrentWaveIndex => currentWaveIndex;
        private int currentWaveIndex = -1;
        
        public int NextWaveIndex => currentWaveIndex + 1;
        public bool HasWaveStarted { get; private set; }

        private readonly List<List<Vector3>> pathsToDraw = new();

        public void StartNextWave() {
            currentWaveIndex = NextWaveIndex;
            Wave wave = waves[currentWaveIndex];
            InitWave(wave);
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
            DrawPaths(pathsToDraw);
            pathsToDraw.Clear();
        }

        private void DrawPath(List<Vector3> pathPoints) {
            LineRenderer pathDrawerLocal = pathDrawerPrefab.GetObject<PathDrawer>(pathDrawersHolder).GetComponent<LineRenderer>();
            pathDrawerLocal.positionCount = 0;

            List<Vector3> pointsToDraw = new List<Vector3>();

            for (int i = 1; i < pathPoints.Count; i++) {
                Vector3 previousPoint = pathPoints[i - 1];
                Vector3 currentPoint = pathPoints[i];
                Vector3 direction = (currentPoint - previousPoint).normalized;

                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(previousPoint, currentPoint, NavMesh.AllAreas, navMeshPath);

                for (int j = 1; j < navMeshPath.corners.Length; j++) {
                    previousPoint = navMeshPath.corners[j - 1];
                    currentPoint = navMeshPath.corners[j];

                    float distance = Vector3.Distance(previousPoint, currentPoint);
                    int numOfFragments = Mathf.CeilToInt(distance / fragmentationDistance);

                    float highestY = Mathf.Max(previousPoint.y, currentPoint.y);
                    float lowestY = Mathf.Min(previousPoint.y, currentPoint.y);

                    for (int k = 0; k <= numOfFragments; k++) {
                        Vector3 fragmentPoint = Vector3.MoveTowards(previousPoint, currentPoint, k * fragmentationDistance);

                        if (Physics.Raycast(new Vector3(fragmentPoint.x, 1000f, fragmentPoint.z), Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << 3)) {

                            if (hit.point.y > highestY) {
                                // If the fragment point y position is higher than the two original points, then make sure that the straight path between those two points is blocked
                                if (!Physics.Raycast(new Vector3(fragmentPoint.x, fragmentPoint.y + fragmentationDistance, fragmentPoint.z), direction, out RaycastHit _, fragmentationDistance, 1 << 3)) continue;
                            }

                            if (hit.point.y < lowestY) {
                                // If the fragment point y position is lower than the two original points, then make sure that the straight path between those two points is blocked
                                if (Physics.Raycast(new Vector3(fragmentPoint.x, fragmentPoint.y + fragmentationDistance, fragmentPoint.z), direction, out RaycastHit _, fragmentationDistance, 1 << 3)) continue;
                            }

                            fragmentPoint.y = hit.point.y + 1;
                        }

                        pointsToDraw.Add(fragmentPoint);
                    }
                }
            }

            StartCoroutine(DrawPathsSlowly(pathDrawerLocal, pointsToDraw));
        }

        private void DrawPaths(List<List<Vector3>> pathsPoints) {
            if (!IsServer) return;

            SerializedNetworkVector3List[] clientPathsPoints = new SerializedNetworkVector3List[pathsPoints.Count];

            for (int i = 0; i < pathsPoints.Count; i++) {
                clientPathsPoints[i] = new SerializedNetworkVector3List(pathsPoints[i].ToArray());
            }

            DrawPathsClientRPC(clientPathsPoints);

            foreach (List<Vector3> pathPoints in pathsPoints) {
                DrawPath(pathPoints);
            }
        }

        [ClientRpc]
        private void DrawPathsClientRPC(SerializedNetworkVector3List[] pathsPoints) {
            foreach (SerializedNetworkVector3List pathPoints in pathsPoints) {
                DrawPath(pathPoints.Objects.ToList());
            }
        }

        /// <summary>
        /// Draw it slowly so the player can watch it while being drawn
        /// </summary>
        /// <param name="pathDrawerLocal"></param>
        /// <param name="pointsToDraw"></param>
        /// <returns></returns>
        private IEnumerator DrawPathsSlowly(LineRenderer pathDrawerLocal, List<Vector3> pointsToDraw) {
            for (int i = 0; i < pointsToDraw.Count; i++) {
                yield return new WaitForSeconds(pathCreationSpeed);
                pathDrawerLocal.positionCount = i + 1;
                pathDrawerLocal.SetPosition(i, pointsToDraw[i]);
            }
        }
    }
}