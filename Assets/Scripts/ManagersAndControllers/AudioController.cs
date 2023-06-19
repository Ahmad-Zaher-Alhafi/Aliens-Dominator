using System;
using System.Collections;
using Context;
using UnityEngine;

namespace ManagersAndControllers {
    public class AudioController : MonoBehaviour {
        private static AudioController instance;

        [SerializeField] private Sound screamSound;
        [SerializeField] private Sound warningSound;
        [SerializeField] private Sound mainMusic;
        [SerializeField] private Sound bgMusic;

        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource bgMusicAudioSource;
        [SerializeField] private AudioSource soundsAudioSource;

        private bool wasScreamSoundPlayed; //to know if the cinematic view was started or not so we do not play the scream and the warning sounds twice 

        private void Awake() {
            PlayBGMusic();
            wasScreamSoundPlayed = false;
            Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
        }

        private void OnEnable() {
            if (instance == null) instance = this;
        }
        
        private void OnWaveStarted() {
            PlayScreamSound();
        }

        private void PlayMainMusic() {
            bgMusicAudioSource.Stop();
            musicAudioSource.clip = mainMusic.audioClip;
            musicAudioSource.volume = mainMusic.volume;
            musicAudioSource.Play();
            musicAudioSource.loop = true;
        }

        private void PlayBGMusic() {
            bgMusicAudioSource.clip = bgMusic.audioClip;
            bgMusicAudioSource.volume = bgMusic.volume;
            bgMusicAudioSource.Play();
            bgMusicAudioSource.loop = true;
        }

        private void PlayScreamSound() {
            if (!wasScreamSoundPlayed) {
                wasScreamSoundPlayed = true;
                soundsAudioSource.PlayOneShot(screamSound.audioClip, screamSound.volume);
                StartCoroutine(PlayWarningSound());
            }
        }

        private IEnumerator PlayWarningSound() {
            yield return new WaitForSeconds(3);
            soundsAudioSource.PlayOneShot(warningSound.audioClip, warningSound.volume);
            yield return new WaitForSeconds(5);
            PlayMainMusic();
            soundsAudioSource.Stop();
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
        }

        [Serializable]
        private class Sound {
            public AudioClip audioClip;
            public float volume;
        }
    }
}