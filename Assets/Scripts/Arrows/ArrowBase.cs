using System.Collections;
using System.Collections.Generic;
using ManagersAndControllers;
using Pool;
using UnityEngine;

namespace Arrows {
    [RequireComponent(typeof(TrailRenderer), typeof(Rigidbody), typeof(AudioSource))]
    public class ArrowBase : PooledObject, IDamager {
        public int Damage => damage;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        [SerializeField] protected float speed = 5;
        [SerializeField] protected List<AudioClip> hitSounds = new();
        [SerializeField] protected AudioClip releaseSound;
        [SerializeField] protected AudioClip knockingSound;
        [Range(1, 5)]
        [SerializeField] private int damage = 1;

        public float TimeToDestroyArrow = 5f;

        //Set the chance of a certain arrow to be added
        [Range(1, 100)]
        public int ChanceOfReceiving = 50;
        protected new AudioSource audio;
        protected Rigidbody body;
        protected GameController GameController;
        protected bool hasCollided;
        protected TrailRenderer trail;

        private void Awake() {
            trail = GetComponent<TrailRenderer>();
            body = GetComponent<Rigidbody>();
            audio = GetComponent<AudioSource>();

            GameController = FindObjectOfType<GameController>();
        }

        private void Update() {
            if (hasCollided == false)
                //Making the arrows move more realistic
                transform.forward = Vector3.Slerp(transform.forward, body.velocity.normalized, 10f * Time.deltaTime);
        }

        protected virtual void OnCollisionEnter(Collision collision) { }

        public void Loose(float drawForce) {
            hasCollided = false;
            trail.enabled = true;
            body.isKinematic = false;
            body.velocity = transform.forward * speed * drawForce;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.detectCollisions = true;
            audio.PlayOneShot(releaseSound);
        }

        public void Knock() {
            hasCollided = false;
            trail.enabled = false;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.detectCollisions = false;
            audio.PlayOneShot(knockingSound);

            body.velocity = Vector3.zero;

            EnableColliders();
        }

        protected IEnumerator DestroyArrow() {
            yield return new WaitForSeconds(TimeToDestroyArrow);

            transform.SetParent(null);
            gameObject.SetActive(false);
            ReturnToPool();
        }

        private void EnableColliders() {
            foreach (Collider component in GetComponentsInChildren<Collider>()) component.enabled = true;
        }

        protected void DisableColliders() {
            foreach (Collider component in GetComponentsInChildren<Collider>()) component.enabled = false;
        }
    }
}