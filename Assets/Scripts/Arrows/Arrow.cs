using UnityEngine;

namespace Arrows {
    public class Arrow : ArrowBase {
        public float speed = 5;

        [SerializeField] private AudioClip hitSound;
        public AudioClip releaseSound;
        public AudioClip knockingSound;
        private new AudioSource audio;
        private Rigidbody body;
        private bool hasCollided;
        private TrailRenderer trail;

        private void Awake() {
            trail = GetComponent<TrailRenderer>();
            body = GetComponent<Rigidbody>();
            audio = GetComponent<AudioSource>();
        }

        private void Update() {
            if (hasCollided == false) transform.LookAt(transform.position + body.velocity * 10);
        }

        private void OnCollisionEnter(Collision collision) {
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (GetComponent<Rigidbody>())
                GetComponent<Rigidbody>().isKinematic = true;

            trail.enabled = false;
            DisableColliders();

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) {
                target.HandleArrowHit(this);
                Debug.Log("Hit");
            } else {
                audio.PlayOneShot(hitSound);
            }

            Destroy(gameObject, 5f);
        }


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
            trail.enabled = false;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.detectCollisions = false;
            audio.PlayOneShot(knockingSound);
        }

        private void DisableColliders() {
            foreach (Collider component in GetComponentsInChildren<Collider>()) component.enabled = false;
        }
    }
}