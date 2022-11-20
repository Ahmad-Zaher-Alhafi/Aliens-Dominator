using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures;
using UnityEngine;

namespace ManagersAndControllers {
    public class CreatureSpawnController : MonoBehaviour {
        [Header("Cinematic wave Setup")]
        [SerializeField] private Wave cinematicWave;

        [Header("Waves Setup")]
        [SerializeField] private List<Wave> waves;

        [SerializeField] private Transform creaturesHolder;

        [Header("Spawn points")]
        [SerializeField] private List<Transform> spawningPoints;

        [Header("Waypoints")]
        [SerializeField] private List<Waypoint> groundCinematicEnemyWaypoints;
        public List<Waypoint> GroundCinematicEnemyWaypoints => groundCinematicEnemyWaypoints;
        [SerializeField] private List<Waypoint> airCinematicEnemyWaypoints;
        public List<Waypoint> AirCinematicEnemyWaypoints => airCinematicEnemyWaypoints;
        // Points where cinematic creatures are gonna run away to on the start of the game
        [SerializeField] private List<Transform> runningAwayPoints;
        public List<Transform> RunningAwayPoints => runningAwayPoints;

        [Header("Spawning Settings")]
        [SerializeField] private float timeBetweenEachCreatureSpawn = 10;

        private readonly List<Creature> creatures = new();
        private bool hasWaveStarted;

        private void Start() {
            SpawnCinematicCreatures();
        }

        private IEnumerator SpawnWaveCreatures(Wave wave) {
            Dictionary<Creature, int> creaturesData = wave.WaveCreatures.ToDictionary(prefab => prefab.CreaturePrefab, num => num.NumberToSpawn);
            List<Creature> waveCreaturesPrefabs = creaturesData.Keys.ToList();

            while (waveCreaturesPrefabs.Count > 0) {
                Transform randomPointToSpawnAt = GetRandomObjectFromList(spawningPoints);
                int randomCreaturePrefabIndex = Random.Range(0, waveCreaturesPrefabs.Count);
                Creature randomCreaturePrefab = waveCreaturesPrefabs[randomCreaturePrefabIndex];

                SpawnCreature(randomCreaturePrefab, randomPointToSpawnAt.transform.position);
                creaturesData[randomCreaturePrefab]--;

                if (creaturesData[randomCreaturePrefab] == 0) {
                    waveCreaturesPrefabs.Remove(randomCreaturePrefab);
                }
                
                yield return new WaitForSeconds(timeBetweenEachCreatureSpawn);
            }
        }

        private void SpawnCreature(Creature creatureToSpawn, Vector3 spawnPosition) {
            Creature creature = creatureToSpawn.GetObject<Creature>(creaturesHolder);
            creature.Init(spawnPosition);
            creatures.Add(creature);
        }

        private void SpawnCinematicCreatures() {
            foreach (Wave.WaveCreature waveCreature in cinematicWave.WaveCreatures) {
                for (int i = 0; i < waveCreature.NumberToSpawn; i++) {
                    Waypoint randomPoint = GetRandomObjectFromList(waveCreature.CreaturePrefab is FlyingCreature ? airCinematicEnemyWaypoints : groundCinematicEnemyWaypoints);
                    SpawnCreature(waveCreature.CreaturePrefab, randomPoint.transform.position);
                }
            }
        }

        private T GetRandomObjectFromList<T>(IReadOnlyList<T> waypointsList) {
            int randomNumber = Random.Range(0, waypointsList.Count + 1);
            randomNumber = Mathf.Clamp(randomNumber, 0, waypointsList.Count - 1);
            T randomPoint = waypointsList[randomNumber];
            return randomPoint;
        }

        public void OnCreatureDeath(Creature creature) {
            if (!hasWaveStarted) {
                hasWaveStarted = true;
                Ctx.Deps.EventsManager.TriggerWaveStarted();
                StartCoroutine(SpawnWaveCreatures(waves[0]));
            }

            creatures.Remove(creature);
        }
    }
}