using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Projectiles {
    public abstract class Projectile : NetworkBehaviour, IDamager {
        [Header("Specifications")]
        [SerializeField]
        protected float speed = 5;

        [SerializeField]
        private int damageCost = 1;
        public int Damage => damageCost;

        [SerializeField] private float pushingForce;
        public float PushingForce => pushingForce;

        public Transform Transform => transform;
        public GameObject GameObject => gameObject;


        protected Rigidbody Rig;
        private float initialSpeed;
        protected Collider Collider;
        private Coroutine destroyAfterTimeCoroutine;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();
        private readonly NetworkVariable<Vector3> networkScale = new();

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) return;

            Destroy(Collider);
            Destroy(Rig);
        }

        protected virtual void Awake() {
            Rig = GetComponent<Rigidbody>();
            Collider = GetComponent<Collider>();
            initialSpeed = speed;
        }

        protected virtual void Update() {
            if (IsSpawned) {
                if (IsServer) {
                    networkPosition.Value = transform.position;
                    networkRotation.Value = transform.rotation;
                    networkScale.Value = transform.localScale;
                } else {
                    if (networkPosition.Value != Vector3.zero) {
                        transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                        transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
                        transform.localScale = networkScale.Value;
                    }
                }
            }
        }

        public virtual void InitDefaults() {
            transform.localScale = Vector3.one;

            if (Rig != null) {
                Rig.velocity = Vector3.zero;
            }
            speed = initialSpeed;
        }

        public virtual void Fire(IDamageable target) {
            DestroyAfterTime(15);
        }

        protected virtual void OnTriggerEnter(Collider other) {
            Collider.enabled = false;
        }

        /// <summary>
        /// To not let the projectile flying in the air for the rest of the game if it does not hit anything
        /// </summary>
        /// <param name="secondsToDestroy"></param>
        protected void DestroyAfterTime(float secondsToDestroy) {
            if (destroyAfterTimeCoroutine != null) {
                StopCoroutine(destroyAfterTimeCoroutine);
            }

            if (secondsToDestroy == 0) {
                Despawn();
            } else {
                destroyAfterTimeCoroutine = StartCoroutine(DestroyAfterTimeDelayed(secondsToDestroy));
            }
        }

        private IEnumerator DestroyAfterTimeDelayed(float secondsToDestroy) {
            yield return new WaitForSeconds(secondsToDestroy);
            Despawn();
        }

        private void Despawn() {
            gameObject.SetActive(false);
            NetworkObject.Despawn(false);
        }
    }
}