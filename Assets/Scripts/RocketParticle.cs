using System.Collections;
using Pool;
using UnityEngine;
public class RocketParticle : PooledObject {
    public ParticleSystem ParticleSystem { get; private set; }

    private void Awake() {
        ParticleSystem = GetComponent<ParticleSystem>();
    }

    public void HideOncePlayingFinished() {
        StartCoroutine(HideAfterTimeDelayed());
    }
    
    private IEnumerator HideAfterTimeDelayed() {
        yield return new WaitForSeconds(ParticleSystem.main.startLifetime.constant);
        ReturnToPool();
    }
}