using System.Collections;
using Audio;
using Pool;
using UnityEngine;
public class RocketParticle : PooledObject {
    public ParticleSystem ParticleSystem { get; private set; }

    private AudioSource audioSource;
    private Sound sound;

    private void Awake() {
        ParticleSystem = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        sound = GetComponent<Sound>();
    }

    public void PlaySound() {
        if (sound == null) return;
        audioSource.PlayOneShot(sound.AudioClip, sound.Volume);
    }

    public void HideOncePlayingFinished() {
        StartCoroutine(HideAfterTimeDelayed());
    }


    private IEnumerator HideAfterTimeDelayed() {
        yield return new WaitForSeconds(ParticleSystem.main.startLifetime.constant);
        ReturnToPool();
    }
}