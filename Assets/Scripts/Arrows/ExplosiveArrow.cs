using System.Collections;
using UnityEngine;

namespace Arrows {
    public class ExplosiveArrow : ArrowBase {
        public float detonationTime = 3f;
        public int impactDamage = 200;
        public float radius = 5f;
        public int hitForce = 1000;

        protected override void OnCollisionEnter(Collision collision) {
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (GetComponent<Rigidbody>())
                GetComponent<Rigidbody>().isKinematic = true;

            trail.enabled = false;
            DisableColliders();

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) target.HandleArrowHit(this);
            else audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);

            StartCoroutine(StartTimer());
        }

        private IEnumerator StartTimer() {
            yield return new WaitForSeconds(detonationTime);

            foreach (GameObject enemy in GameController.AllEnemies.ToArray())
                if (enemy) {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                }

            StartCoroutine(DestroyArrow());
        }
    }
}