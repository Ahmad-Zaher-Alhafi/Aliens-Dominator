using System.Collections;
using Context;
using FMODUnity;
using ScriptableObjects;
using UnityEngine;

namespace ManagersAndControllers {
    public class AudioController : MonoBehaviour {
        private static AudioController instance;

        [SerializeField] private StudioEventEmitter screamSound;
        [SerializeField] private StudioEventEmitter warningSound;
        [SerializeField] private StudioEventEmitter mainMusic;
        [SerializeField] private StudioEventEmitter bgMusic;

        private bool wasScreamSoundPlayed;

        private void Awake() {
            PlayBackgroundMusic();
            wasScreamSoundPlayed = false;
            Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
        }

        private void OnEnable() {
            if (instance == null) instance = this;
        }

        private void OnWaveStarted(Wave wave) {
            PlayScreamSound();
        }

        private void PlayMainMusic() {
            bgMusic.Stop();
            mainMusic.Play();
        }

        private void PlayBackgroundMusic() {
            bgMusic.Play();
        }

        private void PlayScreamSound() {
            if (!wasScreamSoundPlayed) {
                wasScreamSoundPlayed = true;
                screamSound.Play();
                StartCoroutine(PlayWarningSound());
            }
        }

        private IEnumerator PlayWarningSound() {
            yield return new WaitForSeconds(3);
            warningSound.Play();
            yield return new WaitForSeconds(5);
            PlayMainMusic();
            warningSound.Stop();
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
        }
    }
}