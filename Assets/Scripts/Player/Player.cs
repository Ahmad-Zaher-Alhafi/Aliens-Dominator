using Arrows;
using Context;
using FMODUnity;
using Multiplayer;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class Player : NetworkBehaviour {
        [SerializeField] private GameObject arrowPrefab;

        [SerializeField] private float lookSpeed = 3;
        [SerializeField] private float cameraVerticalClamp = 90;
        [SerializeField] private float teleportSpeed;

        [SerializeField] private Transform arrowSpawnPoint;
        public Transform ArrowSpawnPoint => arrowSpawnPoint;

        [SerializeField] private TargetPoint enemyTargetPoint;
        public TargetPoint EnemyTargetPoint => enemyTargetPoint;

        [SerializeField] private Transform bow;
        [SerializeField] private float maxBowMovement = 0.4f;

        [SerializeField] private StudioEventEmitter releaseSound;
        [SerializeField] private StudioEventEmitter drawSound;

        [SerializeField] private Camera playerCamera;

        private Arrow arrow;
        private float draw;
        private Vector3 initialDrawPosition;

        private float initialYRotation;
        private Vector2 rotation = Vector2.zero;
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private bool hasToTeleport;
        private Vector3 teleportPosition;
        private readonly NetworkVariable<Vector3> networkPosition = new();


        private readonly Vector3 serverPosition = new(163, 27, 189);
        private readonly Vector3 clientPosition = new(155, 27, 175);

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                MoveToInstantly();
            }
        }

        private void Start() {
            if (!IsOwner) {
                playerCamera.enabled = false;
            }
        }

        public void Update() {
            if (IsOwner) {
                LookUpdate(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), rotation);

                if (Input.GetButtonDown("Fire1")) {
                    if (IsServer) {
                        SpawnArrow();
                    } else {
                        SpawnArrowServerRPC();
                    }
                }

                if (arrow != null) {
                    if (Input.GetButton("Fire1")) {
                        DrawUpdate();
                    }

                    ReleaseUpdate(Input.GetButtonUp("Fire1"));
                }

                if (hasToTeleport) {
                    MoveTo();
                }
            } else {
                transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
            }
        }

        private void LookUpdate(float inputX, float inputY, Vector2 rotation) {
            float yChange = inputY * lookSpeed;
            rotation.x -= yChange;
            rotation.x = Mathf.Clamp(rotation.x, -cameraVerticalClamp, cameraVerticalClamp);

            float xChange = inputX * lookSpeed;
            rotation.y += xChange;

            this.rotation = rotation;
            transform.rotation = Quaternion.Euler(new Vector2(rotation.x, rotation.y + initialYRotation));

            if (IsServer) {
                networkRotation.Value = transform.rotation;
            } else {
                LookUpdateServerRPC(transform.rotation);
            }
        }

        [ServerRpc]
        private void LookUpdateServerRPC(Quaternion rotation) {
            networkRotation.Value = rotation;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnArrowServerRPC() {
            Arrow spawnedArrow = SpawnArrow();
            ReturnSpawnedArrowClientRPC(new NetworkBehaviourReference(spawnedArrow.GetComponent<NetworkBehaviour>()));
        }

        [ClientRpc]
        private void ReturnSpawnedArrowClientRPC(NetworkBehaviourReference spawnedArrowReference) {
            NetworkBehaviour networkBehaviour = spawnedArrowReference;
            arrow = networkBehaviour as Arrow;
        }

        private Arrow SpawnArrow() {
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            networkObject.SpawnWithOwnership(OwnerClientId);
            networkObject.transform.SetParent(transform);

            arrow = networkObject.GetComponent<Arrow>();
            drawSound.Play();
            return arrow;
        }

        private void DrawUpdate() {
            draw = Mathf.Clamp(draw + Time.deltaTime, 0, 1);
            arrow.transform.position = arrowSpawnPoint.position;

            // Whenever we draw, we want the bow to move as well
            var bowLocalPosition = bow.localPosition;
            float z = Mathf.Clamp(bowLocalPosition.z + draw, -maxBowMovement, maxBowMovement);
            bowLocalPosition = Vector3.Lerp(bowLocalPosition, new Vector3(bowLocalPosition.x, bowLocalPosition.y, z), Time.deltaTime * 4f);
            bow.localPosition = bowLocalPosition;


            // Reset the bow position if its not already reset
            if (bow.localPosition.z != 0f) {
                bow.localPosition = Vector3.Lerp(bow.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }

        private void ReleaseUpdate(bool release) {
            if (release) {
                if (arrow == null) return;

                arrow.Fire(draw);
                arrow = null;
                draw = 0;
                releaseSound.Play();
            }
        }

        public void TeleportTo(Vector3 teleportPosition) {
            hasToTeleport = true;
            this.teleportPosition = teleportPosition;
        }

        private void MoveTo() {
            if (Vector3.Distance(transform.position, teleportPosition) > .2f) {
                transform.position = Vector3.LerpUnclamped(transform.position, teleportPosition, teleportSpeed * Time.deltaTime);
                if (IsClient) {
                    UpdatePositionServerRPC(transform.position);
                }
            } else {
                hasToTeleport = false;
                Ctx.Deps.EventsManager.TriggerPlayerTeleported(teleportPosition);
            }
        }

        private void MoveToInstantly() {
            if (NetworkManager.Singleton.IsServer) {
                transform.position = serverPosition;
                networkPosition.Value = serverPosition;
            } else {
                transform.position = clientPosition;
                UpdatePositionServerRPC(clientPosition);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePositionServerRPC(Vector3 position) {
            networkPosition.Value = position;
        }
    }
}