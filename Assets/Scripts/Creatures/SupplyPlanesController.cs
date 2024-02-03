using System;
using System.Collections.Generic;
using Context;
using Multiplayer;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Creatures {
    public class SupplyPlanesController : NetworkBehaviour {
        [SerializeField] private SupplyPlane supplyPlane;

        private readonly List<(Vector3 position, Vector3 rotation)> planeSpawnPoints = new() {
            new ValueTuple<Vector3, Vector3>(new Vector3(127, 61, -77), Vector3.up * 5),
            new ValueTuple<Vector3, Vector3>(new Vector3(-71, 61, 219), Vector3.up * 98),
            new ValueTuple<Vector3, Vector3>(new Vector3(-18, 61, -43), Vector3.up * 35)
        };

        private void Awake() {
            Ctx.Deps.EventsManager.SupplyBalloonCollected += OnBalloonCollected;
        }

        private void OnBalloonCollected(Constants.SuppliesTypes suppliesTypes) {
            if (IsServer) {
                SpawnSuppliesPlane(suppliesTypes);
            } else {
                SpawnSupplyPlaneServerRPC(suppliesTypes);
            }
        }

        private void SpawnSuppliesPlane(Constants.SuppliesTypes suppliesTypes) {
            int randomIndex = Random.Range(0, planeSpawnPoints.Count);
            Vector3 spawnPosition = planeSpawnPoints[randomIndex].position;
            Vector3 spawnRotation = planeSpawnPoints[randomIndex].rotation;

            NetworkObject plane = NetworkObjectPool.Singleton.GetNetworkObject(supplyPlane.gameObject, spawnPosition, Quaternion.Euler(spawnRotation));
            
            plane.Spawn();
            plane.GetComponent<SupplyPlane>().MoveToDropArea(suppliesTypes);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnSupplyPlaneServerRPC(Constants.SuppliesTypes suppliesTypes) {
            SpawnSuppliesPlane(suppliesTypes);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.SupplyBalloonCollected -= OnBalloonCollected;
        }
    }

}