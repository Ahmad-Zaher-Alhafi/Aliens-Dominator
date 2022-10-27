using UnityEngine;

namespace Arrows {
    public class SlowdownArrow : ArrowBase {
       

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
        }
    }
}