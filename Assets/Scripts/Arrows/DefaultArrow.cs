using UnityEngine;

namespace Arrows {
    public class DefaultArrow : Arrow {
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
                Debug.Log("Hit");
                target.HandleArrowHit(this);
            } else {
                audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);
            }

            StartCoroutine(DestroyArrow());
        }
    }
}