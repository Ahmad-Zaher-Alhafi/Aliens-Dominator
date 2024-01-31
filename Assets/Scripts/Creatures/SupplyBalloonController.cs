using System.Collections.Generic;
using System.Linq;
using Collectables;
using Multiplayer;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Creatures {
    public class SupplyBalloonController : NetworkBehaviour {
        [SerializeField] private List<SupplyBalloon> supplyBalloonPrefabs;

        private void Awake() {
            supplyBalloonPrefabs = supplyBalloonPrefabs.OrderByDescending(balloon => balloon.chanceOfSpawning).ToList();
        }

        public void SpawnBalloon(Vector3 spawnPosition, int chanceToDropBalloon) {
            int randomSpawnChance = Random.Range(0, 101);
            if (randomSpawnChance > chanceToDropBalloon) return;

            int randomBalloonChance = Random.Range(0, 101);
            SupplyBalloon balloonPrefab = supplyBalloonPrefabs.FirstOrDefault(balloon => randomBalloonChance >= balloon.chanceOfSpawning);
            if (balloonPrefab == null) return;

            NetworkObject balloon = NetworkObjectPool.Singleton.GetNetworkObject(balloonPrefab.gameObject, spawnPosition, quaternion.identity);

            if (IsServer) {
                balloon.Spawn();
            } else {
                SpawnBalloonServerRPC(spawnPosition, chanceToDropBalloon);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnBalloonServerRPC(Vector3 spawnPosition, int chanceToDropBalloon) {
            SpawnBalloon(spawnPosition, chanceToDropBalloon);
        }
    }
}