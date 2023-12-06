using Unity.Netcode;
using UnityEngine;

namespace Multiplayer {
    public class HelloWorldPlayer : NetworkBehaviour {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        private static Vector3 serverPosition = new(136, 35, 211);
        private static Vector3 clientPosition = new(142, 35, 247);

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                Move();
            }
        }

        public void Move() {
            if (NetworkManager.Singleton.IsServer) {
                var randomPosition = GetRandomPositionOnPlane(true);
                transform.position = randomPosition;
                Position.Value = randomPosition;
            } else {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default) {
            Position.Value = GetRandomPositionOnPlane(false);
        }

        static Vector3 GetRandomPositionOnPlane(bool isServer) {
            return isServer
                ? new Vector3(serverPosition.x + Random.Range(-3f, 3f), serverPosition.y, serverPosition.z + Random.Range(-3f, 3f))
                : new Vector3(clientPosition.x + Random.Range(-3f, 3f), clientPosition.y, clientPosition.z + Random.Range(-3f, 3f));
        }

        void Update() {
            transform.position = Position.Value;
        }
    }
}