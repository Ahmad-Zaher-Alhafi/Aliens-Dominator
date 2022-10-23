using System;
using System.Collections;
using UnityEngine;

namespace ManagersAndControllers {
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance;

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
        }

        private void OnEnable() {
            if (Instance == null) Instance = this;
        }

        public void PlayMainMusic() {
            bgMusicAudioSource.Stop();
            musicAudioSource.clip = mainMusic.audioClip;
            musicAudioSource.volume = mainMusic.volume;
            musicAudioSource.Play();
            musicAudioSource.loop = true;
        }

        public void PlayBGMusic() {
            bgMusicAudioSource.clip = bgMusic.audioClip;
            bgMusicAudioSource.volume = bgMusic.volume;
            bgMusicAudioSource.Play();
            bgMusicAudioSource.loop = true;
        }

        public void PlayScreamSound() {
            if (!wasScreamSoundPlayed) {
                wasScreamSoundPlayed = true;
                soundsAudioSource.PlayOneShot(screamSound.audioClip, screamSound.volume);
                StartCoroutine(PlayWarningSound());
            }
        }

        public IEnumerator PlayWarningSound() {
            yield return new WaitForSeconds(3);
            soundsAudioSource.PlayOneShot(warningSound.audioClip, warningSound.volume);
            yield return new WaitForSeconds(5);
            PlayMainMusic();
            soundsAudioSource.Stop();
        }

        [Serializable]
        private class Sound {
            public AudioClip audioClip;
            public float volume;
        }
    }
}