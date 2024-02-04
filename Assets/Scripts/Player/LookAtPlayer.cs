using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class LookAtPlayer : NetworkBehaviour {
        private Player player;

        private void Update() {
            if (!IsSpawned) return;
            player ??= FindObjectsOfType<Player>().SingleOrDefault(p => p.OwnerClientId == NetworkManager.LocalClientId);
            if (player == null) return;

            transform.LookAt(player.transform);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.z);
        }
    }
}