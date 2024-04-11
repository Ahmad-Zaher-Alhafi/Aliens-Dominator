using Context;
using UnityEngine;

namespace Player {
    public class LookAtPlayer : MonoBehaviour {
        private Player player;

        private void Update() {
            if (Ctx.Deps.GameController.Player == null) return;

            Transform playerTransform = Ctx.Deps.GameController.Player.transform;
            transform.eulerAngles = Quaternion.LookRotation(transform.position - playerTransform.position).eulerAngles;
        }
    }
}