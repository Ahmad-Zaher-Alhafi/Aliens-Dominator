using System.Collections;
using Audio;
using UnityEngine;

namespace Projectiles {
    public class Rocket : Projectile {
        [SerializeField] private float rotatingSpeed = 15;
        [SerializeField] private float acceleration = 5;

        [Header("Particles")]
        [SerializeField] private RocketParticle launchSmokeParticlePrefab;
        [SerializeField] private Transform launchSmokeParticlePoint;
        [SerializeField] private RocketParticle explosionParticlePrefab;
        [SerializeField] private Transform explosionParticlePoint;

        [Header("Audio files")]
        [SerializeField] private Sound rocketLaunchSound;
        [SerializeField] private Sound rocketExplosionSound;

        private new Collider collider;
        public override bool HasPushingForce => true;

        private IDamageable target;
        private MeshRenderer meshRenderer;
        private bool wasLaunched;
        private RocketParticle launchSmokeParticle;
        private RocketParticle explosionParticle;

        protected override void Awake() {
            base.Awake();
            collider = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public override void InitDefaults(Vector3 initialLocalPosition) {
            base.InitDefaults(initialLocalPosition);
            collider.enabled = true;
            meshRenderer.enabled = true;
            wasLaunched = false;
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
            AudioSource.PlayOneShot(rocketLaunchSound.AudioClip, rocketLaunchSound.Volume);
            this.target = target;
            launchSmokeParticle = launchSmokeParticlePrefab.GetObject<RocketParticle>(transform);
            launchSmokeParticle.transform.position = launchSmokeParticlePoint.position;
            launchSmokeParticle.transform.rotation = launchSmokeParticlePoint.rotation;
            launchSmokeParticle.ParticleSystem.Play();
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

        private void OnTriggerEnter(Collider other) {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable == null) return;

            launchSmokeParticle.transform.parent = null;
            launchSmokeParticle.HideOncePlayingFinished();

            explosionParticle = explosionParticlePrefab.GetObject<RocketParticle>(null);
            explosionParticle.transform.position = explosionParticlePoint.position;
            explosionParticle.transform.rotation = explosionParticlePoint.rotation;
            explosionParticle.ParticleSystem.Play();
            explosionParticle.HideOncePlayingFinished();

            collider.enabled = false;
            meshRenderer.enabled = false;
            AudioSource.PlayOneShot(rocketExplosionSound.AudioClip, rocketExplosionSound.Volume);
            StopAllCoroutines();
            StartCoroutine(DestroyAfterTime(0));
        }
    }
}