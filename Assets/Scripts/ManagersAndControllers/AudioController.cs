using System.Collections;
using Context;
using FMODUnity;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

namespace ManagersAndControllers {
    public class AudioController : NetworkBehaviour {
        [SerializeField] private StudioEventEmitter screamSound;
        [SerializeField] private StudioEventEmitter warningSound;
        [SerializeField] private StudioEventEmitter mainMusic;
        [SerializeField] private StudioEventEmitter bgMusic;

        private bool wasScreamSoundPlayed;

        private void Awake() {
            PlayBackgroundMusic();
            wasScreamSoundPlayed = false;
            if (IsServer) {
                Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
            }
        }

        private void OnWaveStarted(Wave wave) {
            PlayScreamSoundClientRPC();
        }

        private void PlayMainMusic() {
            bgMusic.Stop();
            mainMusic.Play();
        }

        private void PlayBackgroundMusic() {
            bgMusic.Play();
        }

        [ClientRpc]
        private void PlayScreamSoundClientRPC() {
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

        public override void OnDestroy() {
            base.OnDestroy();
            if (IsServer) {
                Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
            }
        }
    }
}