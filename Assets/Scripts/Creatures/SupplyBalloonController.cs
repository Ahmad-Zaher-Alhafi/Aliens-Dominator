using System.Collections.Generic;
using System.Linq;
using Collectables;
using UnityEngine;

namespace Creatures {
    public class SupplyBalloonController : MonoBehaviour {
        [SerializeField] private List<SupplyBalloon> supplyBalloons;

        private void Awake() {
            supplyBalloons = supplyBalloons.OrderByDescending(balloon => balloon.ChanceOfSpawning).ToList();
        }

        public void SpawnBalloon(Vector3 spawnPosition, int chanceToDropBalloon) {
            int randomSpawnChance = Random.Range(0, 101);
            if (randomSpawnChance > chanceToDropBalloon) return;

            int randomBalloonChance = Random.Range(0, 101);
            SupplyBalloon balloonToSpawn = supplyBalloons.FirstOrDefault(balloon => randomBalloonChance >= balloon.ChanceOfSpawning);
            if (balloonToSpawn == null) return;

            SupplyBalloon spawnedBalloon = balloonToSpawn.GetObject<SupplyBalloon>(null);
            spawnedBalloon.Init(spawnPosition);
        }
    }
}