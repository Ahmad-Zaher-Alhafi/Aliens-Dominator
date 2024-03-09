using System.Collections;
using System.Linq;
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
        [Tooltip("The minimum velocity sqrMagnitude required to let the arrow bounce on hitting something")]
        [SerializeField] protected float minVelocityToBounce = 450;

        private Rigidbody rig;
        private TrailRenderer trailRenderer;
        private Player.Player playerOwner;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private void Awake() {
            trailRenderer = GetComponent<TrailRenderer>();
            rig = GetComponent<Rigidbody>();
            triggerCollider = GetComponent<Collider>();
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            playerOwner = FindObjectsOfType<Player.Player>().Single(o => o.GetComponent<NetworkObject>().OwnerClientId == NetworkObject.OwnerClientId);
            Init();
        }

        private void Init() {
            transform.position = playerOwner.ArrowSpawnPoint.position;
            transform.rotation = playerOwner.ArrowSpawnPoint.rotation;

            trailRenderer.enabled = false;
            triggerCollider.enabled = true;

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
            if (IsOwner) {
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
            trailRenderer.enabled = true;
            ActivateLineRendererClientRPC();
        }

        private void OnCollisionEnter(Collision other) {
            if (!IsOwner) return;

            if (rig.velocity.sqrMagnitude > minVelocityToBounce) {
                PlayArrowHitSound();
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!IsOwner) return;

            // We do not want the arrow to hit anything else if it hit the teleport object
            if (other.CompareTag(Constants.TeleportObject)) {
                DestroyInstantly();
                return;
            }

            if (rig.velocity.sqrMagnitude > minVelocityToBounce) return;

            triggerCollider.enabled = false;

            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rig.isKinematic = true;
            rig.useGravity = false;

            trailRenderer.enabled = false;
            PlayArrowHitSound();

            StartCoroutine(DestroyAfterSeconds(timeToDestroyAfterHit));
        }

        public void Fire(float drawForce) {
            if (IsServer) {
                ActivateLineRendererClientRPC();
            } else {
                ActivateLineRendererServerRPC();
            }

            trailRenderer.enabled = true;
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
            DestroyInstantly();
        }

        private void DestroyInstantly() {
            if (IsServer) {
                Despawn();
            } else {
                DespawnServerRPC();
            }
        }

        private void Despawn() {
            gameObject.SetActive(false);
            NetworkObject.Despawn(false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRPC() {
            Despawn();
        }
    }
}