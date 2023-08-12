using System.Collections;
using Pool;
using UnityEngine;

namespace Projectiles {
    public abstract class Projectile : PooledObject, IDamager {
        [Header("Specifications")]
        [SerializeField]
        protected float speed = 5;

        [SerializeField]
        private int damageCost = 1;
        public int Damage => damageCost;

        [SerializeField] private float pushingForce;
        public float PushingForce => pushingForce;

        public Transform Transform => transform;
        public GameObject GameObject => gameObject;


        protected Rigidbody Rig;
        private float initialSpeed;
        protected Collider Collider;

        protected virtual void Awake() {
            Rig = GetComponent<Rigidbody>();
            Collider = GetComponent<Collider>();
            initialSpeed = speed;
        }

        public virtual void InitDefaults(Vector3 initialLocalPosition) {
            transform.localScale = Vector3.one;
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = initialLocalPosition;
            if (Rig != null) {
                Rig.velocity = Vector3.zero;
            }
            speed = initialSpeed;
        }

        public virtual void Fire(IDamageable target) {
            StartCoroutine(DestroyAfterTime(15));
        }

        /// <param name="createPoint">Use it if the projectile has to be created in a specific place like bullet,
        /// On the other hand, Rockets do not need it as they have their own create points</param>
        public void Fire(IDamageable target, Transform createPoint) {
            if (createPoint != null) {
                transform.position = createPoint.position;
                transform.rotation = createPoint.rotation;
            }

            Fire(target);
        }

        protected virtual void OnTriggerEnter(Collider other) {
            Collider.enabled = false;
        }

        // To not let the projectile flying in the air for the rest of the game if it does not hit anything
        protected IEnumerator DestroyAfterTime(float secondsToDestroy) {
            yield return new WaitForSeconds(secondsToDestroy);
            ReturnToPool();
        }
    }
}