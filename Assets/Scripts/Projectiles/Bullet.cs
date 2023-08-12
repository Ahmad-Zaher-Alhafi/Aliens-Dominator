using UnityEngine;

namespace Projectiles {
    public class Bullet : Projectile {
        public override void Fire(IDamageable target) {
            base.Fire(target);
            Rig.AddForce(transform.forward * 200, ForceMode.Impulse);
        }

        protected override void OnTriggerEnter(Collider other) {
            base.OnTriggerEnter(other);
            ReturnToPool();
        }
    }
}