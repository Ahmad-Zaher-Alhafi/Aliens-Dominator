using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using FiniteStateMachine.CreatureStateMachine;
using Multiplayer;
using ScriptableObjects;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using MathUtils = Utils.MathUtils;
using Random = UnityEngine.Random;

namespace ManagersAndControllers {
    public class CreatureSpawnController : NetworkBehaviour {
        [Header("Creatures Prefab")]
        [SerializeField] private CreatureMagantee creatureMaganteePrefab;

        [Space]
        [SerializeField] private bool spawnCinematicCreatures;

        [Header("Cinematic wave Setup")]
        [SerializeField] private Wave cinematicWave;

        [Header("Waves Setup")]
        [SerializeField] private List<Wave> waves;

        [SerializeField] private Transform creaturesHolder;

        [Header("Spawn points")]
        [SerializeField] private List<SpawnPoint> spawningPoints;

        [SerializeField] private SpawnPoint testSpawnPoint;

        [Header("PathPoints")]
        [SerializeField] private List<PathPoint> groundCinematicEnemyPathPoints;
        public List<PathPoint> GroundCinematicEnemyPathPoints => groundCinematicEnemyPathPoints;
        [SerializeField] private List<PathPoint> airCinematicEnemyPathPoints;
        public List<PathPoint> AirCinematicEnemyPathPoints => airCinematicEnemyPathPoints;

        // Points where cinematic creatures are gonna run away when the waves starts
        [Header("Running Away Points")]
        [SerializeField] private List<Transform> runningAwayPoints;
        public List<Transform> RunningAwayPoints => runningAwayPoints;

        [Header("Base's target points")]
        [SerializeField] private List<TargetPoint> attackPoints;

        [FormerlySerializedAs("playerAttackPoint")]
        [Header("Player's target point")]
        [SerializeField] private TargetPoint playerTargetPoint;

        [Header("Spawning Settings")]
        [SerializeField] private float timeBetweenEachCreatureSpawn = 10;

        private readonly List<Creature> creatures = new();
        private bool finishedSpawningCreatures = true;
        public int WavesCount => waves.Count;

        private void Awake() {
            Ctx.Deps.EventsManager.EnemyDied += OnEnemyDied;
            Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            if (!IsServer) return;

            if (spawnCinematicCreatures) {
                StartCoroutine(SpawnCinematicCreaturesDelayed());
            }
        }

        private IEnumerator SpawnCinematicCreaturesDelayed() {
            yield return new WaitForEndOfFrame();
            SpawnCinematicCreatures();
        }

        public void InitWavePaths(int waveIndex) {
            Wave wave = waves[waveIndex];

            for (int i = 0; i < wave.NumOfSpawnPoints; i++) {
                SpawnPoint randomSpawnPoint = MathUtils.GetRandomObjectFromList(spawningPoints);
                SpawnPointPath pathToFollow;
                TargetPoint targetPoint;

                for (int j = 0; j < wave.NumOfGroundPaths; j++) {
                    pathToFollow = MathUtils.GetRandomObjectFromList(randomSpawnPoint.GroundPaths);
                    targetPoint = MathUtils.GetRandomObjectFromList(attackPoints);
                    // Do not draw it if already exists
                    if (wave.AddWavePath(randomSpawnPoint, pathToFollow, true, targetPoint)) continue;
                    DrawPath(randomSpawnPoint.transform.position, pathToFollow.PathPoints.Select(point => point.transform.position).ToList(), targetPoint.transform.position);
                }

                for (int j = 0; j < wave.NumOfAirPaths; j++) {
                    pathToFollow = randomSpawnPoint.AirPath;
                    targetPoint = playerTargetPoint;
                    wave.AddWavePath(randomSpawnPoint, pathToFollow, false, targetPoint);
                }
            }
        }

        private void DrawPath(Vector3 startPoint, List<Vector3> pathPoints, Vector3 endPoint) {
            pathPoints.Insert(0, startPoint);
            pathPoints.Add(endPoint);
            Ctx.Deps.GameController.DrawPath(pathPoints);
        }

        private void SpawnWaveCreatures(int waveIndex) {
            StartCoroutine(SpawnWaveCreatures(waves[waveIndex]));
        }

        private IEnumerator SpawnWaveCreatures(Wave wave) {
            finishedSpawningCreatures = false;

            Dictionary<Creature, int> creaturesData = wave.WaveCreatures.Where(creature => creature.NumberToSpawn > 0)
                .ToDictionary(prefab => prefab.CreaturePrefab, num => num.NumberToSpawn);
            List<Creature> waveCreaturesPrefabs = creaturesData.Keys.ToList();

            while (waveCreaturesPrefabs.Count > 0) {
                // This is a deconstruct statement, do not be confused, it just gives you better reading for the variables of the tuple instead of item1, item2...etc
                (SpawnPoint randomPointToSpawnAt, _, _, TargetPoint targetPoint) = MathUtils.GetRandomObjectFromList(wave.WavePaths);

                int randomCreaturePrefabIndex = Random.Range(0, waveCreaturesPrefabs.Count);
                Creature randomCreaturePrefab = waveCreaturesPrefabs[randomCreaturePrefabIndex];

                SpawnPointPath pathToFollow = MathUtils.GetRandomObjectFromList(randomCreaturePrefab is FlyingCreature
                    ? wave.WavePaths.Where(tuple => tuple.Item1 == randomPointToSpawnAt && !tuple.Item3).Select(tuple => tuple.Item2).ToList()
                    : wave.WavePaths.Where(tuple => tuple.Item1 == randomPointToSpawnAt && tuple.Item3).Select(tuple => tuple.Item2).ToList());

                SpawnCreature(randomCreaturePrefab.gameObject, randomPointToSpawnAt.transform.position, targetPoint, pathToFollow);
                creaturesData[randomCreaturePrefab]--;

                if (creaturesData[randomCreaturePrefab] == 0) {
                    waveCreaturesPrefabs.Remove(randomCreaturePrefab);
                }

                yield return new WaitForSeconds(timeBetweenEachCreatureSpawn);
            }

            finishedSpawningCreatures = true;
        }

        /// <summary>
        /// Used to spawn creature Magantee from creature Magantis' mouth
        /// </summary>
        public CreatureMagantee SpawnCreatureMagantee(Transform spawnPoint, SpawnPointPath pathToFollow = null, CreatureStateType initialCreatureState = CreatureStateType.None, bool isCinematic = false) {
            TargetPoint targetPoint = MathUtils.GetRandomObjectFromList(attackPoints);
            return SpawnCreature(creatureMaganteePrefab.gameObject, spawnPoint.position, targetPoint, pathToFollow, isCinematic, initialCreatureState: initialCreatureState) as CreatureMagantee;
        }

        private Creature SpawnCreature(GameObject creatureToSpawn, Vector3 spawnPosition, TargetPoint targetPoint, SpawnPointPath pathToFollow = null, bool isCinematic = false, CreatureStateType initialCreatureState = CreatureStateType.None) {
            Creature creature = NetworkObjectPool.Singleton.GetNetworkObject(creatureToSpawn, spawnPosition, quaternion.identity).GetComponent<Creature>();
            creature.NetworkObject.Spawn();

            creature.Init(spawnPosition, pathToFollow, isCinematic, targetPoint, initialCreatureState);
            creatures.Add(creature);
            return creature;
        }

        private void SpawnCinematicCreatures() {
            foreach (Wave.WaveCreature waveCreature in cinematicWave.WaveCreatures) {
                for (int i = 0; i < waveCreature.NumberToSpawn; i++) {
                    PathPoint randomPoint = MathUtils.GetRandomObjectFromList(waveCreature.CreaturePrefab is FlyingCreature ? airCinematicEnemyPathPoints : groundCinematicEnemyPathPoints);
                    SpawnCreature(waveCreature.CreaturePrefab.gameObject, randomPoint.transform.position, null, isCinematic: true);
                }
            }
        }

        public List<T> GetCreaturesOfType<T>() where T : Creature {
            return creatures.OfType<T>().ToList();
        }

        private void OnEnemyDied(Creature creature) {
            creatures.Remove(creature);

            if (!finishedSpawningCreatures) return;

            if (creatures.Count(c => !c.IsCinematic) == 0) {
                Ctx.Deps.EventsManager.TriggerWaveFinished();
            }
        }

        private void OnWaveStarted(int waveIndex) {
            SpawnWaveCreatures(waveIndex);
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.EnemyDied -= OnEnemyDied;
            Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
        }

#if UNITY_EDITOR
        [Space, Header("Editor Stuff")]
        [SerializeField, HideInInspector] private Creature testCreaturePrefab;

        [CustomEditor(typeof(CreatureSpawnController))]
        public class CreatureSpawnControllerEditor : Editor {
            private SerializedProperty testCreaturePrefab;

            private void OnEnable() {
                testCreaturePrefab = serializedObject.FindProperty("testCreaturePrefab");
            }

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                serializedObject.Update();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(testCreaturePrefab, new GUIContent("Test creature prefab"));
                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button("Spawn Test Creature")) {
                    if (Application.isPlaying) {
                        CreatureSpawnController creatureSpawnController = (CreatureSpawnController) target;

                        SpawnPointPath pathToFollow = testCreaturePrefab.objectReferenceValue is FlyingCreature
                            ? creatureSpawnController.testSpawnPoint.AirPath
                            : MathUtils.GetRandomObjectFromList(creatureSpawnController.testSpawnPoint.GroundPaths);

                        Creature creature = creatureSpawnController.SpawnCreature((GameObject) testCreaturePrefab.objectReferenceValue, creatureSpawnController.testSpawnPoint.transform.position, MathUtils.GetRandomObjectFromList(creatureSpawnController.attackPoints),
                            pathToFollow, initialCreatureState: CreatureStateType.FollowingPath);
                        creature.tag = "TestCreature";
                        creature.name = "Test Creature";
                        /*creature.GetComponent<CreatureStateMachine>().enabled = false;
                        NavMeshAgent navMeshAgent = creature.GetComponent<NavMeshAgent>();
                        if (navMeshAgent != null) {
                            navMeshAgent.enabled = false;
                        }*/
                    } else {
                        Debug.LogError("Works only in play mode!");
                    }
                }
            }
        }
#endif
    }
}