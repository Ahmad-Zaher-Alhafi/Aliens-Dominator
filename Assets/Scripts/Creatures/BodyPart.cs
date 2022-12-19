using Arrows;
using UnityEngine;

namespace Creatures {
    public class BodyPart : MonoBehaviour {
        public enum CreatureBodyPart {
            Head,
            Body,
            Leg,
            Arm,
            Foot,
            Tail
        }

        [Range(1f, 5f)]
        [SerializeField] private int damageWeight = 1;
        [SerializeField] private CreatureBodyPart type;
        [SerializeField] private Creature creature;

        private new Collider collider;
        private Rigidbody rig;
        
        private void Awake() {
            collider = GetComponent<Collider>();
            rig = GetComponent<Rigidbody>();
        }
        
        public void Init(PhysicMaterial physicMaterial) {
            rig.useGravity = false;
            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            collider.material = physicMaterial;
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.layer == Constants.Arrow_LAYER_ID) {
                creature.TakeDamage(collision.gameObject.GetComponent<ArrowBase>(), damageWeight);
            }
        }
        
        public void OnDeath() {
            rig.useGravity = true;
            rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
    }
}