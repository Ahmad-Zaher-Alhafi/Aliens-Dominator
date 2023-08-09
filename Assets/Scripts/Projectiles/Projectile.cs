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
        public abstract bool HasPushingForce { get; }
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        protected AudioSource AudioSource;
        protected Rigidbody Rig;


        protected virtual void Awake() {
            AudioSource = GetComponent<AudioSource>();
            Rig = GetComponent<Rigidbody>();
        }

        public virtual void InitDefaults(Vector3 initialLocalPosition) {
            transform.localScale = Vector3.one;
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = initialLocalPosition;
            if (Rig != null) {
                Rig.velocity = Vector3.zero;
            }
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

        // To not let the projectile flying in the air for the rest of the game if it does not hit anything
        protected IEnumerator DestroyAfterTime(float secondsToDestroy) {
            yield return new WaitForSeconds(secondsToDestroy);
            ReturnToPool();
        }
    }
}