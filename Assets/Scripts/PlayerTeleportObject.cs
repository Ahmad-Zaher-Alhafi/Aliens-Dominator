using System.Linq;
using Arrows;
using Context;
using UnityEngine;
public class PlayerTeleportObject : MonoBehaviour {
    private void Awake() {
        Ctx.Deps.EventsManager.PlayerTeleported += OnPlayerTeleported;
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.TryGetComponent(out Arrow arrow)) return;

        Player.Player player = FindObjectsOfType<Player.Player>().Single(player1 => player1.OwnerClientId == arrow.OwnerClientId).GetComponent<Player.Player>();
        OrderToTeleport(player);
    }

    private void OrderToTeleport(Player.Player player) {
        player.TeleportTo(transform.position);
        gameObject.SetActive(false);
    }

    private void OnPlayerTeleported(Vector3 teleportPosition) {
        if (teleportPosition == transform.position) return;
        gameObject.SetActive(true);
    }

    private void OnDestroy() {
        Ctx.Deps.EventsManager.PlayerTeleported -= OnPlayerTeleported;
    }
}