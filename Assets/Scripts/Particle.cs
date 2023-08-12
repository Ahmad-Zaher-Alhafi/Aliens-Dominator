using System.Collections;
using FMODUnity;
using Pool;
using UnityEngine;
public class Particle : PooledObject {
    protected ParticleSystem ParticleSystem;
    private StudioEventEmitter sound;

    protected virtual void Awake() {
        ParticleSystem = GetComponent<ParticleSystem>();
        sound = GetComponent<StudioEventEmitter>();
    }

    public void Play() {
        ParticleSystem.Play();
        if (sound != null) {
            sound.Play();
        }
        StartCoroutine(HideAfterTimeDelayed());
    }

    private IEnumerator HideAfterTimeDelayed() {
        yield return new WaitForSeconds(ParticleSystem.main.startLifetime.constant);
        ReturnToPool();
    }
}