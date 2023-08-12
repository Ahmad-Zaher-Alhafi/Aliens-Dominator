using System.Collections;
using FMODUnity;
using Pool;
using UnityEngine;

namespace Arrows {
    public abstract class Arrow : PooledObject, IDamager {
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

        private void Awake() {
            trailRenderer = GetComponent<TrailRenderer>();
            rig = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
        }

        public void Init(Transform spawnPoint) {
            transform.localPosition = Vector3.zero;
            transform.rotation = spawnPoint.rotation;
            
            trailRenderer.enabled = false;
            
            rig.isKinematic = true;
            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rig.detectCollisions = false;
            rig.velocity = Vector3.zero;
            
            collider.enabled = true;
        }

        private void Update() {
            if (!collider.enabled) return;
            // Rotate towards the velocity direction
            transform.forward = Vector3.Slerp(transform.forward, rig.velocity.normalized, 10f * Time.deltaTime);
        }

        protected void OnTriggerEnter(Collider other) {
            collider.enabled = false;

            transform.SetParent(other.transform);

            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rig.isKinematic = true;

            trailRenderer.enabled = false;
            
            arrowHitSound.Play();

            StartCoroutine(Destroy());
        }

        public void Fire(float drawForce) {
            transform.SetParent(null);
            trailRenderer.enabled = true;
            rig.isKinematic = false;
            rig.velocity = transform.forward * speed * drawForce;
            rig.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rig.detectCollisions = true;
        }

        private IEnumerator Destroy() {
            yield return new WaitForSeconds(TimeToDestroyArrow);
            transform.SetParent(null);
            gameObject.SetActive(false);
            ReturnToPool();
        }
    }
}