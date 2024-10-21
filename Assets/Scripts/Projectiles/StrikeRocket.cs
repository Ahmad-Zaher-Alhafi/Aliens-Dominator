using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Projectiles {
    public class StrikeRocket : Projectile {
        [SerializeField] private float damageRadios = 30;
        [SerializeField] private float acceleration = 5;
        [Tooltip("How high the missile arcs in the air")]
        [SerializeField] private float height = 15f;
        [Tooltip("Total time it takes for the missile to reach the target")]
        [SerializeField] private float travelTime = 3f;

        [Header("Particles")]
        [SerializeField] private SmokeParticles launchSmokeParticlePrefab;
        [SerializeField] private Transform launchSmokeParticlePoint;
        [SerializeField] private ExplosionParticle explosionParticlePrefab;
        [SerializeField] private Transform explosionParticlePoint;

        public float ExpectedArriveTime => travelTime / acceleration;

        private MeshRenderer meshRenderer;
        private SmokeParticles launchSmokeParticle;
        private ExplosionParticle explosionParticle;

        private float elapsedTime; // Track how long the missile has been moving.
        private Vector3 targetPosition;
        private Vector3 startPosition; // Where the missile starts from (spawn point).
        private Rigidbody rig;
        private bool isExploded;

        protected override void Awake() {
            base.Awake();
            meshRenderer = GetComponent<MeshRenderer>();
            rig = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsServer) {
                Destroy(rig);
                return;
            }

            InitDefaults();
            Fire(null);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            isExploded = false;
            elapsedTime = 0;
        }

        public override void InitDefaults() {
            base.InitDefaults();
            startPosition = transform.position;
            meshRenderer.enabled = true;
        }

        public void SetTargetPosition(Vector3 targetPosition) {
            this.targetPosition = targetPosition;
        }

        protected override void Update() {
            if (isExploded) return;
            base.Update();
            if (!IsServer) return;

            // Increase elapsed time by how much time has passed in this frame.
            elapsedTime += acceleration * Time.deltaTime;

            // Calculate the percentage of the journey that has been completed.
            float progress = Mathf.Clamp01(elapsedTime / travelTime);

            // Calculate the current position along the parabolic path.
            Vector3 nextPosition = MathUtils.CalculateParabolicPath(startPosition, targetPosition, height, progress);

            // Rotate the missile to face the target (optional for realism).
            Vector3 direction = (nextPosition - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);

            // Move the missile to the calculated position.
            transform.position = nextPosition;

            if (progress >= 1) {
                Explode();
            }
        }

        public override void Fire(IDamageable target) {
            base.Fire(target);
            CreateLaunchSmokeParticleClientRPC();
        }

        [ClientRpc]
        private void CreateLaunchSmokeParticleClientRPC() {
            launchSmokeParticle = launchSmokeParticlePrefab.GetObject<SmokeParticles>(transform);
            launchSmokeParticle.transform.position = launchSmokeParticlePoint.position;
            launchSmokeParticle.transform.rotation = launchSmokeParticlePoint.rotation;
            launchSmokeParticle.Play();
        }

        [ClientRpc]
        private void CreateExplosionParticleClientRPC() {
            explosionParticle = explosionParticlePrefab.GetObject<ExplosionParticle>(transform);
            explosionParticle.transform.position = explosionParticlePoint.position;
            explosionParticle.transform.rotation = explosionParticlePoint.rotation;
            explosionParticle.transform.parent = null;
            explosionParticle.Play();
        }

        [ClientRpc]
        private void ReleaseLaunchParticleFromParentClientRPC() {
            launchSmokeParticle.transform.parent = null;
        }

        protected override void OnTriggerEnter(Collider other) {
            base.OnTriggerEnter(other);
            Explode();
        }

        private void Explode() {
            isExploded = true;

            ReleaseLaunchParticleFromParentClientRPC();
            CreateExplosionParticleClientRPC();

            Collider[] collidedObjects = new Collider[100];
            int numOfOverlappedObjects = Physics.OverlapSphereNonAlloc(transform.position, damageRadios, collidedObjects, LayerMask.GetMask("Enemy"));

            if (numOfOverlappedObjects == 0) {
                DestroyAfterTime(0);
                return;
            }

            foreach (Collider collidedObject in collidedObjects.Where(c => c != null)) {
                if (!collidedObject.TryGetComponent(out IDamageable damageable)) continue;
                damageable.TakeExplosionDamage(this, Damage);
            }

            DestroyAfterTime(0);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadios);
        }
    }
}