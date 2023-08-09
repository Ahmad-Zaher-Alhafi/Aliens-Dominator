using Audio;
using UnityEngine;

namespace Projectiles {
    public class Bullet : Projectile {
        public override bool HasPushingForce => false;

        [Header("Audio files")]
        [SerializeField] private Sound bulletSound;

        public override void Fire(IDamageable target) {
            base.Fire(target);
            Rig.AddForce(transform.forward * 200, ForceMode.Impulse);
            AudioSource.PlayOneShot(bulletSound.AudioClip, bulletSound.Volume);
        }

        private void OnTriggerEnter(Collider other) {
            ReturnToPool();
        }
    }
}