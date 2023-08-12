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
        private CreatureBodyPart Type => type;
        private Creature creature;

        private new Collider collider;
        private Rigidbody Rig { get; set; }
        private Vector3 initialLocalPosition;
        private Quaternion initialLocalRotation;

        private void Awake() {
            creature = GetComponentInParent<Creature>();
            collider = GetComponent<Collider>();
            Rig = GetComponent<Rigidbody>();
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
        }

        public void Init(PhysicMaterial physicMaterial) {
            Rig.useGravity = false;
            Rig.isKinematic = true;
            Rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            Rig.velocity = Vector3.zero;
            collider.material = physicMaterial;
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }

        private void OnTriggerEnter(Collider other) {
            IDamager damager = other.gameObject.GetComponent<IDamager>();
            if (damager == null) return;
            TakeDamage(damager, damageWeight);
        }

        public void OnDeath() {
            Rig.useGravity = true;
            Rig.isKinematic = false;
            Rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (creature.ObjectDamagedWith != null && Type == CreatureBodyPart.Body) {
                // Force to push the creature away and rotate it once get killed (More realistic)
                Rig.AddForce(creature.ObjectDamagedWith.Transform.forward * creature.ObjectDamagedWith.PushingForce, ForceMode.Impulse);
            }
        }

        public void TakeDamage(IDamager damager, int damageWeight) {
            creature.TakeDamage(damager, damageWeight);
        }
    }
}