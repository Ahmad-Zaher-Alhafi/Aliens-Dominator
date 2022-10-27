using System.Collections;
using UnityEngine;

namespace Arrows {
    public class ExplosiveArrow : ArrowBase {
        public float detonationTime = 3f;
        public float impactDamage = 200f;
        public float radius = 5f;
        public float hitForce = 1000f;

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

            foreach (GameObject enemy in GameHandler.AllEnemies.ToArray())
                if (enemy) {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);

                    if (dist <= radius) enemy.GetComponent<Creatures.Creature>().ReceiveExplosionDamage(this, impactDamage, hitForce);
                }

            StartCoroutine(DestroyArrow());
        }
    }
}