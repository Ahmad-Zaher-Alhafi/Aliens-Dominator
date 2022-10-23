using System.Collections;
using UnityEngine;

namespace Particles {
    public class ParticlesDestroyer : MonoBehaviour {
        [SerializeField] private float secondsToDestroy;

        public void DestroyAfterSeconds() {
            StartCoroutine(DestroyAfterTime());
        }

        public IEnumerator DestroyAfterTime() {
            yield return new WaitForSeconds(secondsToDestroy);
            if (gameObject != null) Destroy(gameObject);
        }
    }
}