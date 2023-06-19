using UnityEngine;

namespace Creatures {
    public class BodyPart : MonoBehaviour, IDamageable {
        public enum CreatureBodyPart {
            Head,
            Body,
            Leg,
            Arm,
            Foot,
            Tail
        }

        public GameObject GameObject => gameObject;
        public bool IsDestroyed => creature.IsDead;

        [Range(1f, 5f)]
        [SerializeField] private int damageWeight = 1;
        [SerializeField] private CreatureBodyPart type;
        public CreatureBodyPart Type => type;
        [SerializeField] private Creature creature;

        private new Collider collider;
        private Rigidbody rig;
        private Vector3 initialLocalPosition;
        private Quaternion initialLocalRotation;

        private void Awake() {
            collider = GetComponent<Collider>();
            rig = GetComponent<Rigidbody>();
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
        }

        public void Init(PhysicMaterial physicMaterial) {
            rig.useGravity = false;
            rig.isKinematic = true;
            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            collider.material = physicMaterial;
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }
        
        private void OnTriggerEnter(Collider other) {
            IDamager damager = other.gameObject.GetComponent<IDamager>();
            if (damager == null) return;
            TakeDamage(damager, damageWeight);
        }

        private void OnCollisionEnter(Collision collision) {
            IDamager damager = collision.gameObject.GetComponent<IDamager>();
            if (damager == null) return;
            TakeDamage(damager, damageWeight);
        }

        public void OnDeath() {
            rig.useGravity = true;
            rig.isKinematic = false;
            rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void TakeDamage(IDamager damager, int damageWeight) {
            creature.TakeDamage(damager, damageWeight);
        }
    }
}