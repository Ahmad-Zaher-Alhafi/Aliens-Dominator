using Creature;
using UnityEngine;

namespace Arrows {
    public class ReviveArrow : ArrowBase {
        protected override void OnCollisionEnter(Collision collision) {
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (GetComponent<Rigidbody>()) {
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().useGravity = false;
            }
            trail.enabled = false;
            DisableColliders();

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) {
                var creature = target.GetComponentInParent<Creature.Creature>();

                creature.RigBody.GetComponent<RagdollRewind>().DoRewind();
            } else {
                audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);
            }

            StartCoroutine(DestroyArrow());
        }
    }
}