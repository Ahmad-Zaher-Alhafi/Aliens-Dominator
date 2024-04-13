using System.Collections;
using Context;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;

namespace Arrows {
    public abstract class Arrow : NetworkBehaviour, IDamager {
        public int Damage => damage;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;
        public float PushingForce => pushingForce;
        [SerializeField] private float pushingForce;

        [SerializeField] protected float speed = 5;
        [SerializeField] protected StudioEventEmitter arrowHitSound;
        [Range(1, 5)]
        [SerializeField] private int damage = 1;

        // Used if the arrow hit something
        [SerializeField] float timeToDestroyAfterHit = 5f;
        // Used if the arrow did not hit anything
        [SerializeField] float timeToDestroyAfterFire = 25f;

        [SerializeField] private Collider triggerCollider;

        private Rigidbody rig;
        private TrailRenderer trailRenderer;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private void Awake() {
            trailRenderer = GetComponent<TrailRenderer>();
            rig = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            StartCoroutine(InitDelayed());
        }

        /// <summary>
        /// Init delayed to give time for the Game controller to add the player OnNetworkSpawn
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitDelayed() {
            Init();
            yield return new WaitForEndOfFrame();
            Player.Player ownerPlayer = Ctx.Deps.GameController.GetPlayerOfClientId(OwnerClientId);
            transform.position = ownerPlayer.ArrowSpawnPoint.position;
            transform.rotation = ownerPlayer.ArrowSpawnPoint.rotation;
        }

        private void Init() {
            trailRenderer.enabled = false;
            triggerCollider.enabled = false;

            if (IsOwner) {
                if (rig == null) {
                    rig = gameObject.AddComponent<Rigidbody>();
                }
                rig.isKinematic = true;
                rig.useGravity = false;
                rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rig.detectCollisions = false;
                rig.velocity = Vector3.zero;
            } else {
                // If nt owner destroy the rigid body so it does not interfere with position that is coming from the network (from the owner)
                Destroy(rig);
            }
        }

        private void Update() {
            UpdatePosition();
            Rotate();
        }

        private void UpdatePosition() {
            if (IsOwner) {
                if (IsServer) {
                    networkPosition.Value = transform.position;
                } else {
                    UpdatePositionServerRPC(transform.position);
                }
            } else {
                if (networkPosition.Value != Vector3.zero) {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePositionServerRPC(Vector3 position) {
            networkPosition.Value = position;
        }

        private void Rotate() {
            if (IsOwner && rig != null) {
                // Rotate towards the velocity direction
                transform.forward = Vector3.Slerp(transform.forward, rig.velocity.normalized, 10f * Time.deltaTime);
            }

            if (IsOwner) {
                if (IsServer) {
                    networkRotation.Value = transform.rotation;
                } else {
                    RotateServerRPC(transform.rotation);
                }
            } else {
                if (networkRotation.Value != Quaternion.identity) {
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RotateServerRPC(Quaternion rotation) {
            networkRotation.Value = rotation;
        }

        [ClientRpc]
        private void ActivateLineRendererClientRPC() {
            trailRenderer.enabled = true;
        }

        [ServerRpc]
        private void ActivateLineRendererServerRPC() {
            ActivateLineRendererClientRPC();
        }

        private void OnTriggerEnter(Collider other) {
            if (!IsOwner) return;

            triggerCollider.enabled = false;

            rig.velocity = Vector3.zero;
            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rig.isKinematic = true;
            rig.useGravity = false;

            trailRenderer.enabled = false;
            PlayArrowHitSound();

            if (other.gameObject.layer is not (Constants.Terrain_LAYER_ID or Constants.Base_LAYER_ID)) {
                StartCoroutine(DestroyInstantly());
                return;
            }

            StartCoroutine(DestroyAfterSeconds(timeToDestroyAfterHit));
        }

        public void Fire(float drawForce) {
            if (IsServer) {
                ActivateLineRendererClientRPC();
            } else {
                ActivateLineRendererServerRPC();
            }

            trailRenderer.enabled = true;
            triggerCollider.enabled = true;
            rig.isKinematic = false;
            rig.useGravity = true;
            rig.velocity = transform.forward * speed * drawForce;
            rig.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rig.detectCollisions = true;

            StartCoroutine(DestroyAfterSeconds(timeToDestroyAfterFire));
        }

        private void PlayArrowHitSound() {
            if (IsServer) {
                PlayArrowHitSoundClientRPC();
            } else {
                PlayArrowHitSoundServerRPC();
            }
        }

        [ClientRpc]
        private void PlayArrowHitSoundClientRPC() {
            arrowHitSound.Play();
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayArrowHitSoundServerRPC() {
            PlayArrowHitSoundClientRPC();
        }

        private IEnumerator DestroyAfterSeconds(float timeToDestroyArrow) {
            yield return new WaitForSeconds(timeToDestroyArrow);
            StartCoroutine(DestroyInstantly());
        }

        private IEnumerator DestroyInstantly() {
            // Needed to allow colliding with other objects before being destroyed
            yield return new WaitForFixedUpdate();

            if (IsServer) {
                Despawn();
            } else {
                DespawnServerRPC();
            }
        }

        private void Despawn() {
            NetworkObject.Despawn();
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRPC() {
            Despawn();
        }
    }
}