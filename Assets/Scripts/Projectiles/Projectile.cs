using System;
using System.Collections;
using UnityEngine;

namespace Projectiles {
    public class Projectile : MonoBehaviour, IDamager {
        public bool HasPushingForce => false;
        public int Damage => DamageCost;

        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        public int DamageCost;
        [HideInInspector]
        public bool WasShoot;
        [HideInInspector]
        public bool IsUsed;
        [SerializeField] private AudioSource audioSource;
        public float speed;
        [SerializeField] private float rotatingSpeed = 15;
        [SerializeField] private Vector3 targetOffset;
        [Header("Only for rockets")]
        [SerializeField] private bool isItRocket;
        [SerializeField] private ParticleSystem rocketSmokeParticle;
        [SerializeField] private Sound rocketLaunch;
        [SerializeField] private Sound rocketExplosion;
        [SerializeField] private MeshRenderer rocketMeshRenderer;
        private CapsuleCollider rocketCapsuleCollider;

        private IDamageable target;
        private Vector3 wantedAngle, oldAngle; //wanted angle is the angle that the creature has to rotate to it to reach the wanted point, old angle is the current angle

        private void Start() {
            rocketCapsuleCollider = GetComponent<CapsuleCollider>();
            IsUsed = false;
        }

        private void Update() {
            if (target is null) return;

            transform.position += transform.forward * speed * Time.deltaTime;
            if (!target.IsDestroyed) {
                RotateToTheWantedAngle(target.GameObject);
            }

            rotatingSpeed += Time.deltaTime * 4;
            speed += Time.deltaTime * 2;
        }

        private void OnTriggerEnter(Collider other) {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable == null) return;


            if (isItRocket) {
                if (rocketSmokeParticle != null) {
                    rocketSmokeParticle.transform.parent = null;
                }

                rocketCapsuleCollider.enabled = false;
                rocketMeshRenderer.enabled = false;
                rocketSmokeParticle.GetComponent<DestroyAfterTime>().OrderToDestroy(rocketSmokeParticle.GetComponent<ParticleSystem>().main.startLifetime.constant);
                audioSource.PlayOneShot(rocketExplosion.audioClip, rocketExplosion.volume);
                StartCoroutine(DestroyAfterTime(3));
            } else {
                Destroy(gameObject);
            }

        }

        // To not let the projectile flying in the air for the rest of the game if it does not hit anything
        private IEnumerator DestroyAfterTime(float secondsToDestroy) {
            yield return new WaitForSeconds(secondsToDestroy);
            if (gameObject != null) {
                Destroy(gameObject);
            }
        }

        public void FollowTarget(IDamageable target) {
            transform.parent = null;
            WasShoot = true;
            StartCoroutine(DestroyAfterTime(15));
            audioSource.PlayOneShot(rocketLaunch.audioClip, rocketLaunch.volume);
            this.target = target;
            if (isItRocket) rocketSmokeParticle.Play();
        }

        /// <summary>
        ///     To put the projectile in the right rotation
        /// </summary>
        /// <param name="objectToLookAt">the object that you want the projectile to look at while he is moving</param>
        private void RotateToTheWantedAngle(GameObject objectToLookAt) {
            if (objectToLookAt == null) return;

            Vector3 targetDirection = objectToLookAt.transform.position - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotatingSpeed * Time.deltaTime); //rotate the projectile smoothly from old angle to the new one
        }

        [Serializable]
        private class Sound {
            public AudioClip audioClip;
            public float volume;
        }
    }
}