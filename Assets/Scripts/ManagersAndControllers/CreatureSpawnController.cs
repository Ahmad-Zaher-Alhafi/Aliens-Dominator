using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using Pool;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace ManagersAndControllers {
    public class CreatureSpawnController : MonoBehaviour {
        [Header("Cinematic wave Setup")]
        [SerializeField] private Wave cinematicWave;

        [Header("Waves Setup")]
        [SerializeField] private List<Wave> waves;

        [SerializeField] private Transform creaturesHolder;

        [Header("Spawn points")]
        [SerializeField] private List<SpawnPoint> spawningPoints;

        [Header("PathPoints")]
        [SerializeField] private List<PathPoint> groundCinematicEnemyPathPoints;
        public List<PathPoint> GroundCinematicEnemyPathPoints => groundCinematicEnemyPathPoints;
        [SerializeField] private List<PathPoint> airCinematicEnemyPathPoints;
        public List<PathPoint> AirCinematicEnemyPathPoints => airCinematicEnemyPathPoints;
        // Points where cinematic creatures are gonna run away to on the start of the game
        [SerializeField] private List<Transform> runningAwayPoints;
        public List<Transform> RunningAwayPoints => runningAwayPoints;

        [Header("Spawning Settings")]
        [SerializeField] private float timeBetweenEachCreatureSpawn = 10;
        
        public bool HasWaveStarted { get; private set; }
        

        private readonly List<Creature> creatures = new();

        private void Start() {
            SpawnCinematicCreatures();
        }

        private IEnumerator SpawnWaveCreatures(Wave wave) {
            Dictionary<Creature, int> creaturesData = wave.WaveCreatures.ToDictionary(prefab => prefab.CreaturePrefab, num => num.NumberToSpawn);
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

        private void SpawnCreature(PooledObject creatureToSpawnPool, SpawnPoint spawnPoint) {
            SpawnPointPath pathToFollow = creatureToSpawnPool is FlyingCreature ? spawnPoint.AirPath : MathUtils.GetRandomObjectFromList(spawnPoint.GroundPaths);
            SpawnCreature(creatureToSpawnPool, spawnPoint.transform.position, pathToFollow);
        }

        private void SpawnCreature(PooledObject creatureToSpawnPool, Vector3 spawnPosition, SpawnPointPath pathToFollow = null) {
            Creature creature = creatureToSpawnPool.GetObject<Creature>(creaturesHolder);
            Creature.CreatureState initialState = HasWaveStarted ? Creature.CreatureState.FollowingPath : Creature.CreatureState.Idle;
            creature.Init(spawnPosition, pathToFollow, initialState);
            creatures.Add(creature);
        }

        private void SpawnCinematicCreatures() {
            foreach (Wave.WaveCreature waveCreature in cinematicWave.WaveCreatures) {
                for (int i = 0; i < waveCreature.NumberToSpawn; i++) {
                    PathPoint randomPoint = MathUtils.GetRandomObjectFromList(waveCreature.CreaturePrefab is FlyingCreature ? airCinematicEnemyPathPoints : groundCinematicEnemyPathPoints);
                    SpawnCreature(waveCreature.CreaturePrefab, randomPoint.transform.position);
                }
            }
        }

        public void OnCreatureDeath(Creature creature) {
            if (!HasWaveStarted) {
                HasWaveStarted = true;
                Ctx.Deps.EventsManager.TriggerWaveStarted();
                StartCoroutine(SpawnWaveCreatures(waves[0]));
            }

            creatures.Remove(creature);
        }
    }
}