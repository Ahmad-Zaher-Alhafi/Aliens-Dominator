using Arrows;
using FMODUnity;
using Multiplayer;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class Player : NetworkBehaviour {
        [SerializeField] private GameObject arrowPrefab;

        [SerializeField] private float lookSpeed = 3;
        [SerializeField] private float cameraVerticalClamp = 90;

        [SerializeField] private Transform arrowSpawnPoint;
        public Transform ArrowSpawnPoint => arrowSpawnPoint;

        [SerializeField] private Transform bow;
        [SerializeField] private float maxBowMovement = 0.4f;

        [SerializeField] private StudioEventEmitter releaseSound;
        [SerializeField] private StudioEventEmitter drawSound;

        [SerializeField] private PlayerTeleportObject currentPlayerTeleportObject;
        [SerializeField] private Camera playerCamera;

        private Arrow arrow;
        private float draw;
        private Vector3 initialDrawPosition;
        private float initialYRotation;
        private Vector2 rotation = Vector2.zero;

        private NetworkVariable<Quaternion> networkRotation = new();

        public PlayerTeleportObject CurrentPlayerTeleportObject {
            get => currentPlayerTeleportObject;
            set => currentPlayerTeleportObject = value;
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

                if (arrow == null) return;

                if (Input.GetButton("Fire1")) {
                    DrawUpdate();
                }

                ReleaseUpdate(Input.GetButtonUp("Fire1"));
            } else {
                transform.rotation = networkRotation.Value;
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

            LookUpdateServerRPC(transform.rotation);
        }

        [ServerRpc]
        private void LookUpdateServerRPC(Quaternion rotation) {
            networkRotation.Value = rotation;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnArrowServerRPC() {
            ulong networkObjectId = SpawnArrow().NetworkObjectId;
            UpdateSpawnedArrowClientRPC(networkObjectId);
        }

        private Arrow SpawnArrow() {
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            networkObject.SpawnWithOwnership(OwnerClientId);
            networkObject.transform.SetParent(transform);

            arrow = networkObject.GetComponent<Arrow>();
            drawSound.Play();
            return arrow;
        }

        [ClientRpc]
        private void UpdateSpawnedArrowClientRPC(ulong networkObjectId) {
            arrow = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<Arrow>();
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
    }
}