using UnityEngine;

namespace Arrows {
    public class ReviveArrow : Arrow {
        protected override void OnCollisionEnter(Collision collision) {
            base.OnCollisionEnter(collision);
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (GetComponent<Rigidbody>()) {
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().useGravity = false;
            }
            trail.enabled = false;

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) {
                var creature = target.GetComponentInParent<Creatures.Creature>();
            }

            StartCoroutine(DestroyArrow());
        }
    }
}