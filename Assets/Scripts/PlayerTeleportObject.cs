using UnityEngine;
public class PlayerTeleportObject : MonoBehaviour {
    [SerializeField] private Player.Player player;
    [SerializeField] private Transform pointToTeleportPlayerTo;
    [SerializeField] private float teleportSpeed;
    private bool hasToTeleport;
    private MeshRenderer meshRenderer;
    private Vector3 teleportPosition; //position of the teleport object the the player wants to move to

    private void Start() {
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
        if (Vector3.Distance(player.transform.position, teleportPosition) > .2f) {
            player.transform.position = Vector3.Lerp(player.transform.position, teleportPosition, teleportSpeed /** Time.deltaTime*/ / Vector3.Distance(player.transform.position, teleportPosition));
        } else {
            player.CurrentPlayerTeleportObject.gameObject.SetActive(true);
            player.CurrentPlayerTeleportObject = this;

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