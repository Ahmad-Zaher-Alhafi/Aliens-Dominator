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

            OrderToSpawnCreatures();
        }

        public void OrderToSpawnCreatures() {
            StartCoroutine(SpawnCreatures());
        }

        private IEnumerator SpawnCreatures() {
            do {
                oldSpeed = nPCSimplePatrol.NavMeshAgent.speed;
                nPCSimplePatrol.NavMeshAgent.speed = 0;

                for (int i = 0; i < numOfCreaturesToSpawn; i++) {
                    animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.SpawnFromMouth));
                    yield return new WaitForSeconds(.3f);
                    if (creature.CurrentState == Creature.CreatureState.Dead) break;
                    var navMeshAgent = creatureSpawnController.SpawnBug(creatureSpawningPoint, bugPrefabToSpawn.gameObject, nPCSimplePatrol).GetComponent<NavMeshAgent>();
                    BugSpawnParticles.Play();
                    navMeshAgent.enabled = false;
                    navMeshAgent.GetComponent<Animator>().Play(Constants.GetAnimationName(navMeshAgent.gameObject.name, Constants.AnimationsTypes.SpawnFromMouth));
                    yield return new WaitForSeconds(.7f);
                    navMeshAgent.enabled = true;
                    yield return new WaitForSeconds(secondsBetweenEachCreatureSpawning);
                }
                nPCSimplePatrol.NavMeshAgent.speed = oldSpeed;

                yield return new WaitForSeconds(secondsBetweenEachSpawnOrder);
            } while (creature.CurrentState != Creature.CreatureState.Dead);
        }
    }
}