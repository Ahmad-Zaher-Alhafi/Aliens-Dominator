using System.Collections;
using FMODUnity;
using Pool;
using UnityEngine;
public class RocketParticle : PooledObject {
    public ParticleSystem ParticleSystem { get; private set; }

    private StudioEventEmitter sound;

    private void Awake() {
        ParticleSystem = GetComponent<ParticleSystem>();
        sound = GetComponent<StudioEventEmitter>();
    }

    public void PlaySound() {
        if (sound == null) return;
        sound.Play();
    }

    public void HideOncePlayingFinished() {
        StartCoroutine(HideAfterTimeDelayed());
    }


    private IEnumerator HideAfterTimeDelayed() {
        yield return new WaitForSeconds(ParticleSystem.main.startLifetime.constant);
        ReturnToPool();
    }
}