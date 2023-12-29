using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using Multiplayer;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace ManagersAndControllers {
    public class GameController : NetworkBehaviour {
        [Header("Nav Mesh")]
        [SerializeField] private List<NavMeshSurface> navMeshSurfaces;

        [Header("Path drawer")]
        [SerializeField] private PathDrawer pathDrawerPrefab;
        [SerializeField] private Transform pathDrawersHolder;
        [SerializeField] private float fragmentationDistance = 3;
        [SerializeField] private float pathCreationSpeed = .5f;

        public int CurrentWaveIndex => currentWaveIndex;
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
            StartCoroutine(StartNextWaveDelayed());
        }

        private void OnWaveFinished() {
            Debug.Log($"Wave {currentWaveIndex} has been finished");

            if (NextWaveIndex >= Ctx.Deps.WaveController.WavesCount) {
                Debug.Log($"Game over, You win");
                return;
            }

            StartNextWave();
        }

        private IEnumerator StartNextWaveDelayed() {
            Assert.IsTrue(NextWaveIndex < Ctx.Deps.WaveController.WavesCount, "No next wave found");

            yield return new WaitForEndOfFrame();


            HasWaveStarted = true;
            Ctx.Deps.WaveController.StartWave(NextWaveIndex);
            currentWaveIndex = NextWaveIndex;
            Debug.Log($"Wave {currentWaveIndex} has been started");
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

            StartCoroutine(SetPositionsDelayed(pathDrawerLocal, pointsToDraw));
        }

        public void DrawPaths(List<List<Vector3>> pathsPoints) {
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

        private IEnumerator SetPositionsDelayed(LineRenderer pathDrawerLocal, List<Vector3> pointsToDraw) {
            for (int i = 0; i < pointsToDraw.Count; i++) {
                yield return new WaitForSeconds(pathCreationSpeed);
                pathDrawerLocal.positionCount = i + 1;
                pathDrawerLocal.SetPosition(i, pointsToDraw[i]);
            }
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