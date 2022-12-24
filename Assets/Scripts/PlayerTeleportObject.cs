using Player;
using UnityEngine;
public class PlayerTeleportObject : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private Transform pointToTeleportPlayerTo;
    [SerializeField] private float teleportSpeed;
    private PlayerController playerController;
    private bool hasToTeleport;
    private MeshRenderer meshRenderer;
    private Vector3 teleportPosition; //position of the teleport object the the player wants to move to

    private void Start() {
        playerController = player.GetComponent<PlayerController>();
        meshRenderer = GetComponent<MeshRenderer>();
        hasToTeleport = false;
    }


    private void Update() {
        if (hasToTeleport) Teleport();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(Constants.Arrow)) {
            Destroy(other.gameObject);
            OrderToTeleport();
        }
    }

    private void Teleport() {
        if (Vector3.Distance(player.position, teleportPosition) > .2f) {
            player.position = Vector3.Lerp(player.position, teleportPosition, teleportSpeed /** Time.deltaTime*/ / Vector3.Distance(player.position, teleportPosition));
        } else {
            playerController.CurrentPlayerTeleportObject.gameObject.SetActive(true);
            playerController.CurrentPlayerTeleportObject = this;

            hasToTeleport = false;
            gameObject.SetActive(false);
            meshRenderer.enabled = true;
        }
    }

    private void OrderToTeleport() {
        meshRenderer.enabled = false;
        teleportPosition = transform.position;
        hasToTeleport = true;
    }
}