using System.Collections.Generic;
using Arrows;
using Cinemachine;
using Context;
using FMODUnity;
using Multiplayer;
using Unity.Netcode;
using UnityEngine;
using Utils.Extensions;

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

        [Header("Bow laser")]
        [SerializeField] private LineRenderer bowLaser;
        [SerializeField] private int laserNumPoints = 50; // Number of points to draw the line
        [SerializeField] private float distanceBetweenLaserPoints = 0.1f; // Time step for each point in the trajectory
        [SerializeField] private Vector2 laserPointDistanceRange = new(0, 100);
        [SerializeField] private Vector2 laserPointScaleRange = new(0.1f, 10);

        private Arrow arrow;
        private Vector3 initialDrawPosition;

        private float initialYRotation;
        private Vector2 rotation = Vector2.zero;
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private bool hasToTeleport;
        private Vector3 teleportPosition;
        private readonly NetworkVariable<Vector3> networkPosition = new();

        private readonly Vector3 serverPosition = new(163, 27, 189);
        private readonly Vector3 clientPosition = new(155, 27, 175);
        private Bow bowComponent;

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

        private void Awake() {
            bowComponent = bow.GetComponentInParent<Bow>();
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

                if (Input.GetButtonDown("Fire1")) {
                    if (drawSound.IsPlaying()) {
                        drawSound.Stop();
                    }
                    drawSound.Play();

                    if (arrow == null) {
                        if (IsServer) {
                            SpawnArrow(OwnerClientId);
                        } else {
                            SpawnArrowServerRPC(OwnerClientId);
                        }
                    }
                }

                if (Input.GetButtonUp("Fire1")) {
                    if (drawSound.IsPlaying()) {
                        drawSound.Stop();
                    }
                }

                DrawBow(Input.GetButton("Fire1"));

                if (arrow != null && Input.GetButtonUp("Fire1")) {
                    ReleaseBow();
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
            transform.rotation = Quaternion.LerpUnclamped(transform.rotation, Quaternion.Euler(new Vector2(rotation.x, rotation.y + initialYRotation)), .1f);

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

        private void DrawBow(bool isDrawing) {
            DrawLaser(isDrawing);

            // Whenever we draw, we want the bow to move as well
            float drawZValue = Mathf.Clamp(bow.localPosition.z + bowComponent.DrawForce, 0, maxBowMovement);
            bow.localPosition = Vector3.Lerp(bow.localPosition, new Vector3(bow.localPosition.x, bow.localPosition.y, drawZValue), 4 * Time.deltaTime);

            if (isDrawing) return;
            // Reset the bow position if its not already reset
            if (bow.localPosition.z != 0f) {
                bow.localPosition = Vector3.Lerp(bow.localPosition, Vector3.zero, 4 * Time.deltaTime);
            }
        }

        private void ReleaseBow() {
            // To prevent spamming arrows
            if (bowComponent.DrawForce < 1 / arrowsPerSecond) return;

            arrow.Fire(bowComponent.DrawForce);
            arrow = null;
            releaseSound.Play();
            BowLaserHitPoint.Instance.gameObject.SetActive(false);
        }

        private void DrawLaser(bool isDrawing) {
            if (!isDrawing || arrow == null) {
                bowLaser.gameObject.SetActiveWithCheck(false);
                return;
            }

            bowLaser.gameObject.SetActiveWithCheck(true);

            List<Vector3> points = new();

            for (int i = 0; i < laserNumPoints; i++) {
                float time = i * distanceBetweenLaserPoints;
                Vector3 point = CalculatePositionAtTime(bowLaser.transform.position, bowLaser.transform.forward * bowComponent.DrawForce * arrow.Speed, time);

                if (i == 0) {
                    points.Add(point);
                    continue;
                }

                Ray ray = new Ray(points[i - 1], point - points[i - 1]);

                if (Physics.Raycast(ray, out RaycastHit rayCastHit, Vector3.Distance(point, points[i - 1]), ~(1 << LayerMask.NameToLayer("Ignore Raycast")))) {
                    points.Add(rayCastHit.point);

                    Vector3 attachmentPoint = rayCastHit.point;
                    Vector3 attachmentPointNormal = rayCastHit.normal;
                    BowLaserHitPoint.Instance.transform.SetPositionAndRotation(attachmentPoint, Quaternion.LookRotation(attachmentPointNormal, rayCastHit.transform.forward));
                    BowLaserHitPoint.Instance.gameObject.SetActiveWithCheck(true);
                    break;
                }

                points.Add(point);
                BowLaserHitPoint.Instance.gameObject.SetActiveWithCheck(false);
            }

            bowLaser.positionCount = points.Count;
            bowLaser.SetPositions(points.ToArray());

            // Calculate the distance between the player and the object
            float distance = Vector3.Distance(transform.position, BowLaserHitPoint.Instance.transform.position);

            // Normalize the distance within the range [minDistance, maxDistance]
            float normalizedDistance = Mathf.InverseLerp(laserPointDistanceRange.x, laserPointDistanceRange.y, distance);

            // Calculate the scale factor based on the normalized distance
            float scaleFactor = Mathf.Lerp(laserPointScaleRange.x, laserPointScaleRange.y, normalizedDistance);

            // Apply the scale to the object
            BowLaserHitPoint.Instance.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);


            Vector3 CalculatePositionAtTime(Vector3 startPosition, Vector3 startVelocity, float time) {
                // Position at time t = start + velocity * t + 0.5 * gravity * t^2
                Vector3 gravity = Physics.gravity;
                Vector3 position = startPosition + startVelocity * time + 0.5f * gravity * time * time;
                return position;
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