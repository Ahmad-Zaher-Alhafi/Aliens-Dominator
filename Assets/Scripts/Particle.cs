using System.Collections;
using FMODUnity;
using Pool;
using UnityEngine;
public class Particle : PooledObject {
    [SerializeField] private bool autoHideAfterLifeTime = true;
    [SerializeField] private bool autoHideWhenNoParticlesLeft;

    private ParticleSystem particlesSystem;
    private StudioEventEmitter sound;

    private bool ShouldBeHidden => autoHideWhenNoParticlesLeft && !particlesSystem.isPlaying && particlesSystem.particleCount == 0;

    protected virtual void Awake() {
        particlesSystem = GetComponent<ParticleSystem>();
        sound = GetComponent<StudioEventEmitter>();
    }

    private void Update() {
        if (ShouldBeHidden) {
            Hide();
        }
    }

    public void Play() {
        particlesSystem.Play();
        if (sound != null) {
            sound.Play();
        }

        if (autoHideAfterLifeTime) {
            StartCoroutine(HideAfterTimeDelayed());
        }
    }

    public void Stop() {
        particlesSystem.Stop();
    }

    private IEnumerator HideAfterTimeDelayed() {
        yield return new WaitForSeconds(particlesSystem.main.startLifetime.constant);
        Hide();
    }

    private void Hide() {
        ReturnToPool();
    }
}