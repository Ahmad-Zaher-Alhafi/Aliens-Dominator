using System.Collections;
using ManagersAndControllers;
using UnityEngine;
using UnityEngine.AI;

namespace Creatures {
    public interface IBugsSpawningCreature { }

    public class BugsSpawningCreature : Creature, IBugsSpawningCreature {
        [Header("For Bugs Spawner Only")]
        [SerializeField] private float numOfCreaturesToSpawn;
        [SerializeField] private Creatures.Creature bugPrefabToSpawn;
        [SerializeField] private float secondsBetweenEachCreatureSpawning;

        [SerializeField] private Transform creatureSpawningPoint;
        public ParticleSystem BugSpawnParticles;
        [Header("For bugs spawner Boss only")]
        [SerializeField] private float secondsBetweenEachSpawnOrder;
        private Animator animator;
        private Creatures.Creature creature;
        private GroundCreatureMover nPCSimplePatrol;

        private float oldSpeed;
        private CreatureSpawnController creatureSpawnController;

        private void Start() {
            creature = GetComponent<Creatures.Creature>();
            animator = GetComponent<Animator>();
            creatureSpawnController = FindObjectOfType<CreatureSpawnController>();
            nPCSimplePatrol = GetComponent<GroundCreatureMover>();
            
        }
    }
}