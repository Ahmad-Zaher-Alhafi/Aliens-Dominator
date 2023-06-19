using System;
using System.Collections.Generic;
using Creatures;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(menuName = "ScriptableObjects/Wave", fileName = "Wave")]
    public class Wave : ScriptableObject {
        [Serializable]
        public class WaveCreature {
            [SerializeField] private Creature creaturePrefab;
            [SerializeField] private int numberToSpawn;
            public Creature CreaturePrefab => creaturePrefab;
            public int NumberToSpawn => numberToSpawn;
        }

        [SerializeField] private List<WaveCreature> waveCreatures;
        public List<WaveCreature> WaveCreatures => waveCreatures;
    }
}