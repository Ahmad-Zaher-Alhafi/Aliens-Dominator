using Arrows;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class PlayerController : NetworkBehaviour {
        [SerializeField] private float networkInterpolationSpeed = .1f;
        [SerializeField] private float lookSpeed = 3;
        [SerializeField] private float cameraVerticalClamp = 90;

        [SerializeField] private Arrow defaultArrow;
        [SerializeField] private Transform arrowSpawnPoint;

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

        private readonly NetworkVariable<Vector2> networkRotation = new();
        private readonly NetworkVariable<Vector3> networkEulerAngle = new();
        private readonly NetworkVariable<Quaternion> networkCameraRotation = new();

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
                HandleLookUpdateServerRPC(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), rotation);
            }


            rotation = networkRotation.Value;
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(transform.eulerAngles), Quaternion.Euler(networkEulerAngle.Value), networkInterpolationSpeed);
            playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, networkCameraRotation.Value, networkInterpolationSpeed);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HandleLookUpdateServerRPC(float inputX, float inputY, Vector2 rotation) {
            LookUpdate(inputX, inputY, rotation);
        }

        private void LookUpdate(float inputX, float inputY, Vector2 rotation) {
            float yChange = inputY * lookSpeed;
            rotation.x -= yChange;
            rotation.x = Mathf.Clamp(rotation.x, -cameraVerticalClamp, cameraVerticalClamp);

            float xChange = inputX * lookSpeed;
            rotation.y += xChange;
            networkRotation.Value = rotation;

            networkEulerAngle.Value = new Vector2(0, rotation.y + initialYRotation);
            networkCameraRotation.Value = Quaternion.Euler(rotation.x, 0, 0);
        }

            transform.eulerAngles = new Vector2(0, rotation.y + initialYRotation);
            mainCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        [ServerRpc(RequireOwnership = false)]
        private void HandleDrawUpdateServerRPC(bool draw) {
            DrawUpdate(draw);
        }
        
        private void DrawUpdate() {
            if (Input.GetButton("Fire1")) {
                if (arrow == null) {
                    arrow = defaultArrow.GetObject<DefaultArrow>(arrowSpawnPoint);
                    arrow.Init(arrowSpawnPoint);
                    drawSound.Play();
                }

                draw = Mathf.Clamp(draw + Time.deltaTime, 0, 1);
                arrow.transform.position = arrowSpawnPoint.position;

                // Whenever we draw, we want the bow to move as well
                var bowLocalPosition = bow.localPosition;
                float z = Mathf.Clamp(bowLocalPosition.z + draw, -maxBowMovement, maxBowMovement);
                bowLocalPosition = Vector3.Lerp(bowLocalPosition, new Vector3(bowLocalPosition.x, bowLocalPosition.y, z), Time.deltaTime * 4f);
                bow.localPosition = bowLocalPosition;
            } else if (Input.GetButtonUp("Fire1")) {
                if (arrow == null) return;

                arrow.Fire(draw);
                arrow = null;
                draw = 0;
                releaseSound.Play();
            }
        }
    }
}