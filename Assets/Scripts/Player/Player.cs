using System.Collections;
using Arrows;
using Cinemachine;
using Context;
using FMODUnity;
using Multiplayer;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class Player : NetworkBehaviour {
        [SerializeField] private GameObject arrowPrefab;

        [SerializeField] private float arrowsPerSecond = 5;
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

        [SerializeField] private CinemachineVirtualCamera playerCamera;
        public CinemachineVirtualCamera PlayerCamera => playerCamera;

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

        private float startDrawTime;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            if (IsOwner) {
                MoveToInstantly();
            }

            Ctx.Deps.EventsManager.TriggerPlayerSpawnedOnNetwork(this);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            Ctx.Deps.EventsManager.TriggerPlayerDespawnedFromNetwork(this);
        }

        private void Start() {
            if (!IsOwner) {
                playerCamera.enabled = false;
            }
        }

        public void Update() {
            if (Ctx.Deps.CameraController.IsBlending) return;

#if UNITY_EDITOR
            if (Cursor.lockState != CursorLockMode.Locked) return;
#endif

            if (IsOwner) {
                LookUpdate(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), rotation);

                if (Input.GetButtonDown("Fire1") && arrow == null) {
                    drawSound.Play();

                    if (IsServer) {
                        SpawnArrow(OwnerClientId);
                    } else {
                        SpawnArrowServerRPC(OwnerClientId);
                    }
                }

                if (arrow != null) {
                    DrawUpdate(Input.GetButton("Fire1"));

                    if (Input.GetButtonUp("Fire1")) {
                        ReleaseUpdate();
                    }
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
        private void SpawnArrowServerRPC(ulong ownerClientId) {
            Arrow spawnedArrow = SpawnArrow(ownerClientId);
            ReturnSpawnedArrowClientRPC(new NetworkBehaviourReference(spawnedArrow.GetComponent<NetworkBehaviour>()), ownerClientId);
        }

        [ClientRpc]
        private void ReturnSpawnedArrowClientRPC(NetworkBehaviourReference spawnedArrowReference, ulong ownerClientId) {
            if (OwnerClientId != ownerClientId) return;
            NetworkBehaviour networkBehaviour = spawnedArrowReference;
            arrow = networkBehaviour as Arrow;
        }

        private Arrow SpawnArrow(ulong ownerClientId) {
            Player ownerPlayer = Ctx.Deps.GameController.GetPlayerOfClientId(ownerClientId);
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(arrowPrefab, ownerPlayer.ArrowSpawnPoint.position, ownerPlayer.ArrowSpawnPoint.rotation);
            networkObject.SpawnWithOwnership(ownerClientId);

            arrow = networkObject.GetComponent<Arrow>();
            return arrow;
        }

        private void DrawUpdate(bool isDrawing) {
            if (isDrawing && startDrawTime == 0) {
                startDrawTime = Time.time;
            }

            int direction = isDrawing ? 1 : -1;
            draw = Mathf.Clamp(draw + Time.deltaTime * direction, 0, 1);
            arrow.transform.position = arrowSpawnPoint.position;
            arrow.transform.rotation = arrowSpawnPoint.rotation;

            // Whenever we draw, we want the bow to move as well
            var bowLocalPosition = bow.localPosition;
            float z = Mathf.Clamp(bowLocalPosition.z + draw, 0, maxBowMovement);
            bowLocalPosition = Vector3.Lerp(bowLocalPosition, new Vector3(bowLocalPosition.x, bowLocalPosition.y, z), Time.deltaTime * 4f);
            bow.localPosition = bowLocalPosition;


            // Reset the bow position if its not already reset
            if (bow.localPosition.z != 0f) {
                bow.localPosition = Vector3.Lerp(bow.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }

        private void ReleaseUpdate() {
            // To prevent spamming arrows
            if (Time.time < startDrawTime + 1 / arrowsPerSecond) {
                startDrawTime = 0;
                return;
            }

            arrow.Fire(draw);
            arrow = null;
            startDrawTime = 0;
            releaseSound.Play();
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