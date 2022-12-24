using UnityEngine;

namespace Arrows {
    public class HypnotizeArrow : Arrow {
        protected override void OnCollisionEnter(Collision collision) {
            base.OnCollisionEnter(collision);
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (GetComponent<Rigidbody>())
                GetComponent<Rigidbody>().isKinematic = true;

            trail.enabled = false;

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) {
                var creature = target.GetComponentInParent<Creatures.Creature>();

                if (creature.gameObject.tag == "OnStartWaves") {
                    target.HandleArrowHit(this);
                    return;
                }
            } else {
                audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);
            }

            StartCoroutine(DestroyArrow());
        }
    }
}