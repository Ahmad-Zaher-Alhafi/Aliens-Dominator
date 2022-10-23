using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BugsSpawner : MonoBehaviour
{
    [Header("For Bugs Spawner Only")]
    [SerializeField] private float numOfCreaturesToSpawn;
    [SerializeField] private Creature bugPrefabToSpawn;
    [SerializeField] private float secondsBetweenEachCreatureSpawning;

    [SerializeField] private Transform creatureSpawningPoint;
    public ParticleSystem BugSpawnParticles;
    [Header("For bugs spawner Boss only")]
    [SerializeField] private float secondsBetweenEachSpawnOrder;

    private float oldSpeed;
    private NPCSimplePatrol nPCSimplePatrol;
    private Spawner spawner;
    private Animator animator;
    private Creature creature;

    private void Start()
    {
        creature = GetComponent<Creature>();
        animator = GetComponent<Animator>();
        spawner = FindObjectOfType<Spawner>();
        nPCSimplePatrol = GetComponent<NPCSimplePatrol>();

        if (creature.IsItBoss)
        {
            OrderToSpawnCreatures(creature.IsItBoss);
        }
    }

    public void OrderToSpawnCreatures(bool isItBoss)
    {
        StartCoroutine(SpawnCreatures(isItBoss));
    }

    private IEnumerator SpawnCreatures(bool isITBoss)
    {
        do
        {
            oldSpeed = nPCSimplePatrol.NavMeshAgent.speed;
            nPCSimplePatrol.NavMeshAgent.speed = 0;

            for (int i = 0; i < numOfCreaturesToSpawn; i++)
            {
                animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.SpawnFromMouth));
                yield return new WaitForSeconds(.3f);
                if (creature.IsDead)
                {
                    break;
                }
                NavMeshAgent navMeshAgent = spawner.SpawnBug(creatureSpawningPoint, bugPrefabToSpawn.gameObject, nPCSimplePatrol).GetComponent<NavMeshAgent>();
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