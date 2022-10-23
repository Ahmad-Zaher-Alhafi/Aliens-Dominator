using System.Collections;
using Creature;
using UnityEngine;
using UnityEngine.AI;

namespace ManagersAndControllers {
    public class BugsSpawner : MonoBehaviour {
        [Header("For Bugs Spawner Only")]
        [SerializeField] private float numOfCreaturesToSpawn;
        [SerializeField] private Creature.Creature bugPrefabToSpawn;
        [SerializeField] private float secondsBetweenEachCreatureSpawning;

        [SerializeField] private Transform creatureSpawningPoint;
        public ParticleSystem BugSpawnParticles;
        [Header("For bugs spawner Boss only")]
        [SerializeField] private float secondsBetweenEachSpawnOrder;
        private Animator animator;
        private Creature.Creature creature;
        private NPCSimplePatrol nPCSimplePatrol;

        private float oldSpeed;
        private Spawner spawner;

        private void Start() {
            creature = GetComponent<Creature.Creature>();
            animator = GetComponent<Animator>();
            spawner = FindObjectOfType<Spawner>();
            nPCSimplePatrol = GetComponent<NPCSimplePatrol>();

            if (creature.IsItBoss) OrderToSpawnCreatures(creature.IsItBoss);
        }

        public void OrderToSpawnCreatures(bool isItBoss) {
            StartCoroutine(SpawnCreatures(isItBoss));
        }

        private IEnumerator SpawnCreatures(bool isITBoss) {
            do {
                oldSpeed = nPCSimplePatrol.NavMeshAgent.speed;
                nPCSimplePatrol.NavMeshAgent.speed = 0;

                for (int i = 0; i < numOfCreaturesToSpawn; i++) {
                    animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.SpawnFromMouth));
                    yield return new WaitForSeconds(.3f);
                    if (creature.IsDead) break;
                    var navMeshAgent = spawner.SpawnBug(creatureSpawningPoint, bugPrefabToSpawn.gameObject, nPCSimplePatrol).GetComponent<NavMeshAgent>();
                    BugSpawnParticles.Play();
                    navMeshAgent.enabled = false;
                    navMeshAgent.GetComponent<Animator>().Play(Constants.GetAnimationName(navMeshAgent.gameObject.name, Constants.AnimationsTypes.SpawnFromMouth));
                    yield return new WaitForSeconds(.7f);
                    navMeshAgent.enabled = true;
                    yield return new WaitForSeconds(secondsBetweenEachCreatureSpawning);
                }
                nPCSimplePatrol.NavMeshAgent.speed = oldSpeed;

                yield return new WaitForSeconds(secondsBetweenEachSpawnOrder);
            } while (isITBoss && !creature.IsDead);
        }
    }
}