using System.Collections;
using Context;
using FMOD.Studio;
using FMODUnity;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

namespace ManagersAndControllers {
    public class AudioController : NetworkBehaviour {
        [Header("Emitters")]
        [SerializeField] private StudioEventEmitter screamSound;
        [SerializeField] private StudioEventEmitter warningSound;
        [SerializeField] private StudioEventEmitter mainMusic;
        [SerializeField] private StudioEventEmitter bgMusic;

        [Header("Volumes")]
        [Range(0, 1)]
        [SerializeField] private float initialMusicVolume = .5f;
        [Range(0, 1)]
        [SerializeField] private float initialSfxVolume = .5f;

        public float MusicBusVolume {
            get {
                 musicBus.getVolume(out float volume);
                 return volume;
            }
        }

        public float SfxBusVolume {
            get {
                sfxBus.getVolume(out float volume);
                return volume;
            }
        }

        private bool wasScreamSoundPlayed;
        private Bus musicBus;
        private Bus sfxBus;

        private void Awake() {
            musicBus = RuntimeManager.GetBus(Constants.FmodMusicBus);
            sfxBus = RuntimeManager.GetBus(Constants.FmodSfxBus);

            SetMusicVolume(PlayerPrefSettings.GetMusicVolume() ?? initialMusicVolume);
            SetSfxVolume(PlayerPrefSettings.GetSfxVolume() ?? initialSfxVolume);

            PlayBackgroundMusic();
            wasScreamSoundPlayed = false;
        }

        public void SetMusicVolume(float volume) {
            musicBus.setVolume(volume);
            PlayerPrefSettings.SetMusicVolume(volume);
        }

        public void SetSfxVolume(float volume) {
            sfxBus.setVolume(volume);
            PlayerPrefSettings.SetSfxVolume(volume);
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) {
                Ctx.Deps.EventsManager.WaveStarted += OnWaveStarted;
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (IsServer) {
                Ctx.Deps.EventsManager.WaveStarted -= OnWaveStarted;
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
    }
}