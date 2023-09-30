using System;
using System.Collections.Generic;
using Creatures;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Wave", fileName = "Wave")]
    public class Wave : ScriptableObject {
        [Header("Num of paths where the creatures will come from")]
        [SerializeField] private int numOfSpawnPoints = 2;
        public int NumOfSpawnPoints => numOfSpawnPoints;

        [SerializeField] private int numOfGroundPaths = 2;
        public int NumOfGroundPaths => numOfGroundPaths;

        [SerializeField] private int numOfAirPaths = 1;
        public int NumOfAirPaths => numOfAirPaths;

        [Serializable]
        public class WaveCreature {
            [SerializeField] private Creature creaturePrefab;
            [SerializeField] private int numberToSpawn;
            public Creature CreaturePrefab => creaturePrefab;
            public int NumberToSpawn => numberToSpawn;
        }

        [SerializeField] private List<WaveCreature> waveCreatures;
        public List<WaveCreature> WaveCreatures => waveCreatures;

        public List<Tuple<SpawnPoint, SpawnPointPath, bool, TargetPoint>> WavePaths { get; } = new();

        public bool AddWavePath(SpawnPoint spawnPoint, SpawnPointPath wavePath, bool isGroundPath, TargetPoint targetPoint) {
            Tuple<SpawnPoint, SpawnPointPath, bool, TargetPoint> tuple = new Tuple<SpawnPoint, SpawnPointPath, bool, TargetPoint>(spawnPoint, wavePath, isGroundPath, targetPoint);
            // Already exists
            if (WavePaths.Contains(tuple)) return true;
            WavePaths.Add(tuple);
            return false;
        }
    }
}