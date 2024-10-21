using System;
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
        public int Health => creature.Health;

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
        private bool isCoreJoint;
        /// <summary>
        /// If ture, then the owner will send some of its transform's properties to other clients on network
        /// </summary>
        private bool hasToSyncMotion;
        private IDamager objectDamagedWith;

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        private void Awake() {
            creature = GetComponentInParent<Creature>();
            collider = GetComponent<Collider>();
            Rig = GetComponent<Rigidbody>();
            characterJoint = GetComponent<CharacterJoint>();
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
            isCoreJoint = GetComponentInParent<BodyPart>() == null;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsServer) {
                if (characterJoint != null) {
                    Destroy(characterJoint);
                }
                Destroy(Rig);
            }
        }

        public void Init(PhysicMaterial physicMaterial) {
            hasToSyncMotion = true;
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
                if (!hasToSyncMotion) return;

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
            objectDamagedWith = other.gameObject.GetComponent<IDamager>();

            if (objectDamagedWith == null) return;

            if (IsServer) {
                TakeDamage(objectDamagedWith.Damage, type, objectDamagedWith.GameObject.GetComponent<NetworkObject>().OwnerClientId);
            } else {
                TakeDamageServerRPC(objectDamagedWith.Damage, objectDamagedWith.GameObject.GetComponent<NetworkObject>().OwnerClientId);
            }
        }

        public void OnDeath() {
            collider.enabled = true;
            Rig.useGravity = true;
            Rig.isKinematic = false;
            if (creature is FlyingCreature) {
                Rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if (!isCoreJoint) {
                Rig.velocity = Vector3.zero;
                Rig.angularVelocity = Vector3.zero;
            }

            if (objectDamagedWith != null) {
                // Force to push the creature away and rotate it once get killed (More realistic)
                Rig.AddForce((creature.transform.position - objectDamagedWith.Transform.position).normalized * objectDamagedWith.PushingForce, ForceMode.Impulse);
            }
        }

        public void OnDespawn() {
            hasToSyncMotion = false;
            if (IsServer) {
                networkPosition.Value = Vector3.zero;
                networkRotation.Value = Quaternion.identity;
            }
        }

        public void TakeDamage(int damage, Enum creatureBodyPart = default, ulong objectDamagedWithClientID = default) {
            if (creature.IsDead) return;
            creature.TakeDamage(damage * damageWeight, type, objectDamagedWithClientID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRPC(int damage, ulong objectDamagedWithClientID) {
            TakeDamage(damage, type, objectDamagedWithClientID);
        }

        public void TakeExplosionDamage(IDamager damager, int damage) {
            objectDamagedWith = damager;
            TakeDamage(damage);
        }
    }
}