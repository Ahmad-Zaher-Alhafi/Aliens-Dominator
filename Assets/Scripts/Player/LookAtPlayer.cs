using Context;
using ManagersAndControllers;
using UnityEngine;

namespace Player {
    public class LookAtPlayer : MonoBehaviour {
        private Player player;

        private void Update() {
            player = Ctx.Deps.GameController.GetPlayerOfClientId(GameController.OwnerClientID);
            if (player == null) return;

            Transform playerTransform = player.transform;
            transform.eulerAngles = Quaternion.LookRotation(transform.position - playerTransform.position).eulerAngles;
        }
    }
}