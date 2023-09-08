using System;
using System.Collections;
using Context;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;

namespace Creatures {
    public class CreatureMagantis : GroundCreature {
        [Header("Magantee spawn specifications")]
        [SerializeField] private Transform maganteeSpawnPoint;
        [SerializeField] private int numOfMaganteeToSpawn;
        [SerializeField] private float delayBetweenEachMaganteeSpawn;

        public override void ExecuteSpecialAbility(Action<bool> informAnimationFinishedCallback) {
            base.ExecuteSpecialAbility(informAnimationFinishedCallback);
            StartCoroutine(SpawnMaganteeCreaturesDelayed());
        }

        private IEnumerator SpawnMaganteeCreaturesDelayed() {
            for (int i = 0; i < numOfMaganteeToSpawn; i++) {
                yield return new WaitForSeconds(delayBetweenEachMaganteeSpawn);
                // Inform call back only when the last creature spawn animation is finished
                Animator.PlaySpecialAbilityAnimation(i < numOfMaganteeToSpawn - 1 ? null : InformAnimationFinishedCallback);
            }
        }

        public void SpawnCreature() {
            Ctx.Deps.CreatureSpawnController.SpawnCreatureMagantee(maganteeSpawnPoint, CreatureStateType.Spawning);
        }
    }
}