using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using FiniteStateMachine.CreatureStateMachine;
using Pool;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using MathUtils = Utils.MathUtils;
using Random = UnityEngine.Random;

namespace ManagersAndControllers {
    public class CreatureSpawnController : MonoBehaviour {
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

        private void Start() {
            if (spawnCinematicCreatures) {
                SpawnCinematicCreatures();
            }
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
                SpawnPoint randomPointToSpawnAt = MathUtils.GetRandomObjectFromList(spawningPoints);
                int randomCreaturePrefabIndex = Random.Range(0, waveCreaturesPrefabs.Count);
                Creature randomCreaturePrefab = waveCreaturesPrefabs[randomCreaturePrefabIndex];

                SpawnCreature(randomCreaturePrefab, randomPointToSpawnAt);
                creaturesData[randomCreaturePrefab]--;

                if (creaturesData[randomCreaturePrefab] == 0) {
                    waveCreaturesPrefabs.Remove(randomCreaturePrefab);
                }

                yield return new WaitForSeconds(timeBetweenEachCreatureSpawn);
            }

            finishedSpawningCreatures = true;
        }

        private Creature SpawnCreature(PooledObject creatureToSpawnPool, SpawnPoint spawnPoint, CreatureStateType initialCreatureState = CreatureStateType.None, bool isCinematic = false) {
            SpawnPointPath pathToFollow;
            TargetPoint targetPoint;

            if (creatureToSpawnPool is FlyingCreature) {
                pathToFollow = spawnPoint.AirPath;
                targetPoint = playerTargetPoint;
            } else {
                pathToFollow = MathUtils.GetRandomObjectFromList(spawnPoint.GroundPaths);
                targetPoint = MathUtils.GetRandomObjectFromList(attackPoints);
            }

            return SpawnCreature(creatureToSpawnPool, spawnPoint.transform.position, targetPoint, pathToFollow, isCinematic, initialCreatureState: initialCreatureState);
        }

        /// <summary>
        /// Used to spawn creature Magantee from creature Magantis' mouth
        /// </summary>
        public void SpawnCreatureMagantee(Transform spawnPoint, CreatureStateType initialCreatureState = CreatureStateType.None, bool isCinematic = false) {
            TargetPoint targetPoint = MathUtils.GetRandomObjectFromList(attackPoints);
            SpawnCreature(creatureMaganteePrefab, spawnPoint.position, targetPoint, null, isCinematic, initialCreatureState: initialCreatureState);
        }

        private Creature SpawnCreature(PooledObject creatureToSpawnPool, Vector3 spawnPosition, TargetPoint targetPoint, SpawnPointPath pathToFollow = null, bool isCinematic = false, CreatureStateType initialCreatureState = CreatureStateType.None) {
            Creature creature = creatureToSpawnPool.GetObject<Creature>(creaturesHolder);
            if (initialCreatureState == CreatureStateType.None) {
                initialCreatureState = Ctx.Deps.GameController.HasWaveStarted ? CreatureStateType.FollowingPath : CreatureStateType.Patrolling;
            }

            creature.Init(spawnPosition, pathToFollow, isCinematic, targetPoint, initialCreatureState);
            creatures.Add(creature);
            return creature;
        }

        private void SpawnCinematicCreatures() {
            foreach (Wave.WaveCreature waveCreature in cinematicWave.WaveCreatures) {
                for (int i = 0; i < waveCreature.NumberToSpawn; i++) {
                    PathPoint randomPoint = MathUtils.GetRandomObjectFromList(waveCreature.CreaturePrefab is FlyingCreature ? airCinematicEnemyPathPoints : groundCinematicEnemyPathPoints);
                    SpawnCreature(waveCreature.CreaturePrefab, randomPoint.transform.position, null, isCinematic: true);
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
                DrawDefaultInspector();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(testCreaturePrefab, new GUIContent("Test creature prefab"));
                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button("Spawn Test Creature")) {
                    if (Application.isPlaying) {
                        CreatureSpawnController creatureSpawnController = (CreatureSpawnController) target;

                        Creature creature = creatureSpawnController.SpawnCreature((Creature) testCreaturePrefab.objectReferenceValue, creatureSpawnController.testSpawnPoint, CreatureStateType.FollowingPath);
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