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

        public float TimeToDestroyArrow = 5f;

        private Rigidbody rig;
        private TrailRenderer trailRenderer;
        private new Collider collider;
        private Player.Player playerOwner;

        private readonly NetworkVariable<Quaternion> networkRotation = new();
        private readonly NetworkVariable<Vector3> networkPosition = new();

        private void Awake() {
            trailRenderer = GetComponent<TrailRenderer>();
            rig = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
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
            collider.enabled = true;

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
            if (!collider.enabled) return;

            UpdatePosition();
            Rotate();
        }

        private void UpdatePosition() {
            if (IsServer) {
                if (IsOwner) {
                    networkPosition.Value = transform.position;
                } else {
                    transform.position = networkPosition.Value;
                }
            } else {
                if (IsOwner) {
                    UpdatePositionServerRPC(transform.position);
                } else {
                    transform.position = networkPosition.Value;
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

            if (IsServer) {
                if (IsOwner) {
                    networkRotation.Value = transform.rotation;
                } else {
                    transform.rotation = networkRotation.Value;
                }
            } else {
                if (IsOwner) {
                    RotateServerRPC(transform.rotation);
                } else {
                    transform.rotation = networkRotation.Value;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RotateServerRPC(Quaternion rotation) {
            networkRotation.Value = rotation;
        }

        protected void OnTriggerEnter(Collider other) {
            collider.enabled = false;


            if (rig != null) {
                rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rig.isKinematic = true;
                rig.useGravity = false;
            }

            trailRenderer.enabled = false;

            arrowHitSound.Play();

            StartCoroutine(Destroy());
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeParentServerRPC() {
            transform.SetParent(null);
        }

        public void Fire(float drawForce) {
            if (IsServer) {
                transform.SetParent(null);
            } else {
                ChangeParentServerRPC();
            }
            trailRenderer.enabled = true;
            rig.isKinematic = false;
            rig.useGravity = true;
            rig.velocity = transform.forward * speed * drawForce;
            rig.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rig.detectCollisions = true;
        }

        private IEnumerator Destroy() {
            yield return new WaitForSeconds(TimeToDestroyArrow);

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