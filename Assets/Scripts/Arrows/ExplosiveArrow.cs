using System.Collections;
using UnityEngine;

namespace Arrows {
    public class ExplosiveArrow : Arrow {
        public float detonationTime = 3f;
        public int impactDamage = 200;
        public float radius = 5f;
        public int hitForce = 1000;

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
            if (target != null) target.HandleArrowHit(this);
        }
    }
}