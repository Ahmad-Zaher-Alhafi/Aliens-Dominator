using Context;
using UnityEngine;

namespace Player {
    public class LookAtPlayer : MonoBehaviour {
        private Player player;

        private void Update() {
            if (Ctx.Deps.GameController.Player == null) return;

            transform.eulerAngles = Quaternion.LookRotation(transform.position - Ctx.Deps.CameraController.LocalActiveCameraPosition).eulerAngles;
        }
    }
}