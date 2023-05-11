using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using Pool;
using ScriptableObjects;
using FiniteStateMachine.States;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;

namespace ManagersAndControllers {
    public class CreatureSpawnController : MonoBehaviour {
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

        public bool HasWaveStarted { get; private set; }


        private readonly List<Creature> creatures = new();


        private void Start() {
            if (spawnCinematicCreatures) {
                SpawnCinematicCreatures();
            }
        }

#if UNITY_EDITOR
        private void Update() {
            if (Input.GetMouseButtonDown(2)) {
                SpawnTestGroundCreature();
            }
        }
#endif

        private IEnumerator SpawnWaveCreatures(Wave wave) {
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
        }

        private void SpawnCreature(PooledObject creatureToSpawnPool, SpawnPoint spawnPoint, StateType initialState = StateType.None, bool isCinematic = false) {
            SpawnPointPath pathToFollow;
            TargetPoint targetPoint;

            if (creatureToSpawnPool is FlyingCreature) {
                pathToFollow = spawnPoint.AirPath;
                targetPoint = playerTargetPoint;
            } else {
                pathToFollow = MathUtils.GetRandomObjectFromList(spawnPoint.GroundPaths);
                targetPoint = MathUtils.GetRandomObjectFromList(attackPoints);
            }

            SpawnCreature(creatureToSpawnPool, spawnPoint.transform.position, targetPoint, pathToFollow, isCinematic, initialState: initialState);
        }

        private void SpawnCreature(PooledObject creatureToSpawnPool, Vector3 spawnPosition, TargetPoint targetPoint, SpawnPointPath pathToFollow = null, bool isCinematic = false, StateType initialState = StateType.None) {
            Creature creature = creatureToSpawnPool.GetObject<Creature>(creaturesHolder);
            if (initialState == StateType.None) {
                initialState = HasWaveStarted ? StateType.FollowingPath : StateType.Patrolling;
            }

            creature.Init(spawnPosition, pathToFollow, isCinematic, targetPoint, initialState);
            creatures.Add(creature);
        }

        private void SpawnCinematicCreatures() {
            foreach (Wave.WaveCreature waveCreature in cinematicWave.WaveCreatures) {
                for (int i = 0; i < waveCreature.NumberToSpawn; i++) {
                    PathPoint randomPoint = MathUtils.GetRandomObjectFromList(waveCreature.CreaturePrefab is FlyingCreature ? airCinematicEnemyPathPoints : groundCinematicEnemyPathPoints);
                    SpawnCreature(waveCreature.CreaturePrefab, randomPoint.transform.position, null, isCinematic: true);
                }
            }
        }

        public void OnCreatureDeath(Creature creature) {
            creatures.Remove(creature);
        }

        public void OnCreatureHit() {
            if (HasWaveStarted) return;
            HasWaveStarted = true;
            Ctx.Deps.EventsManager.TriggerWaveStarted();
            StartCoroutine(SpawnWaveCreatures(waves[0]));
        }

#if UNITY_EDITOR
        private void SpawnTestGroundCreature() {
            Creature groundCreature = waves[0].WaveCreatures[0].CreaturePrefab;
            SpawnCreature(groundCreature, testSpawnPoint, StateType.FollowingPath);
        }
#endif
    }
}