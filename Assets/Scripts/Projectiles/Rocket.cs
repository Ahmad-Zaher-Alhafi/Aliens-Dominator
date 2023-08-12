using UnityEngine;

namespace Projectiles {
    public class Rocket : Projectile {
        [SerializeField] private float rotatingSpeed = 15;
        [SerializeField] private float acceleration = 5;

        [Header("Particles")]
        [SerializeField] private RocketParticle launchSmokeParticlePrefab;
        [SerializeField] private Transform launchSmokeParticlePoint;
        [SerializeField] private ExplosionParticle explosionParticlePrefab;
        [SerializeField] private Transform explosionParticlePoint;

        private IDamageable target;
        private MeshRenderer meshRenderer;
        private bool wasLaunched;
        private RocketParticle launchSmokeParticle;
        private ExplosionParticle explosionParticle;

        protected override void Awake() {
            base.Awake();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public override void InitDefaults(Vector3 initialLocalPosition) {
            base.InitDefaults(initialLocalPosition);
            meshRenderer.enabled = true;
            wasLaunched = false;
            Collider.enabled = false;
        }

        protected void Update() {
            if (!wasLaunched) return;

            transform.position += transform.forward * speed * Time.deltaTime;
            speed += acceleration * Time.deltaTime;

            if (target?.IsDestroyed == false) {
                RotateToTheWantedAngle(target.GameObject);
                rotatingSpeed += Time.deltaTime * 4;
            }
        }

        public override void Fire(IDamageable target) {
            base.Fire(target);
            wasLaunched = true;
            transform.parent = null;
            this.target = target;
            Collider.enabled = true;

            launchSmokeParticle = launchSmokeParticlePrefab.GetObject<RocketParticle>(transform);
            launchSmokeParticle.transform.position = launchSmokeParticlePoint.position;
            launchSmokeParticle.transform.rotation = launchSmokeParticlePoint.rotation;
            launchSmokeParticle.Play();
        }

        /// <summary>
        /// To change the rocket's direction towards the target
        /// </summary>
        /// <param name="target">The object that you want the projectile to look at while it is moving</param>
        private void RotateToTheWantedAngle(GameObject target) {
            if (target == null) return;

            Vector3 targetDirection = target.transform.position - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotatingSpeed * Time.deltaTime);
        }

        protected override void OnTriggerEnter(Collider other) {
            base.OnTriggerEnter(other);
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable == null) return;

            launchSmokeParticle.transform.parent = null;

            explosionParticle = explosionParticlePrefab.GetObject<ExplosionParticle>(transform);
            explosionParticle.transform.position = explosionParticlePoint.position;
            explosionParticle.transform.rotation = explosionParticlePoint.rotation;
            explosionParticle.transform.parent = null;
            explosionParticle.Play();

            meshRenderer.enabled = false;
            StopAllCoroutines();
            StartCoroutine(DestroyAfterTime(0));
        }
    }
}