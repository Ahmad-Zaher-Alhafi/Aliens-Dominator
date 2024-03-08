using Unity.Netcode;
using UnityEngine;

namespace Creatures {
    public class BodyPart : NetworkBehaviour, IDamageable {
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
        private CharacterJoint characterJoint;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private void Awake() {
            creature = GetComponentInParent<Creature>();
            collider = GetComponent<Collider>();
            Rig = GetComponent<Rigidbody>();
            characterJoint = GetComponent<CharacterJoint>();
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsServer) {
                if (characterJoint != null) {
                    Destroy(characterJoint);
                }
                Destroy(Rig);
                Destroy(collider);
            }
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

        private void Update() {
            if (!creature.IsDead) return;

            if (IsServer) {
                networkPosition.Value = transform.position;
                networkRotation.Value = transform.rotation;
            } else {
                if (networkPosition.Value != Vector3.zero) {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!IsServer) return;
            if (creature.IsDead) return;
            IDamager damager = other.gameObject.GetComponent<IDamager>();
            if (damager == null) return;
            TakeDamage(damager, damageWeight);
        }

        public void OnDeath() {
            collider.enabled = true;
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