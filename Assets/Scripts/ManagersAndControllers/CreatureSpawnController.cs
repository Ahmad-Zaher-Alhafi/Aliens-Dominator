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
using MathUtils = Utils.MathUtils;
using Random = UnityEngine.Random;

namespace ManagersAndControllers {
    public class CreatureSpawnController : NetworkBehaviour {
        [Header("Creatures Prefab")]
        [SerializeField] private CreatureMagantee creatureMaganteePrefab;

        [Space]
        [SerializeField] private bool spawnCinematicCreatures;

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

        [Header("Spawning Settings")]
        [SerializeField] private float timeBetweenEachCreatureSpawn = 10;

        private readonly List<Creature> creatures = new();
        private bool finishedSpawningCreatures = true;
        private TargetPoint playerTargetPoint;

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

        /// <summary>
        /// Need to wait a frame until the pool finishes registering the prefabs on spawn
        /// </summary>
        /// <returns></returns>
        private IEnumerator SpawnCinematicCreaturesDelayed() {
            yield return new WaitForEndOfFrame();
            SpawnCinematicCreatures();
        }

        private void SpawnWaveCreatures(Wave wave) {
            if (!IsServer) return;
            StartCoroutine(SpawnWaveCreaturesDelayed(wave));
        }

        private IEnumerator SpawnWaveCreaturesDelayed(Wave wave) {
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
            (_, _, _, TargetPoint targetPoint) = MathUtils.GetRandomObjectFromList(Ctx.Deps.WaveController.CurrentWave.WavePaths);
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
            foreach (Wave.WaveCreature waveCreature in Ctx.Deps.WaveController.CinematicWave.WaveCreatures) {
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

        private void OnWaveStarted(Wave wave) {
            SpawnWaveCreatures(wave);
        }

        public override void OnDestroy() {
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

                        (_, _, _, TargetPoint targetPoint) = MathUtils.GetRandomObjectFromList(Ctx.Deps.WaveController.CurrentWave.WavePaths);

                        Creature creature = creatureSpawnController.SpawnCreature((GameObject) testCreaturePrefab.objectReferenceValue, creatureSpawnController.testSpawnPoint.transform.position, targetPoint,
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