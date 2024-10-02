using Context;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class SettingsMenu : MonoBehaviour {
        [Header("Sliders")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Volumes")]
        [Range(0, 1)]
        [SerializeField] private float maxMusicVolume = 1;

        [Range(0, 1)]
        [SerializeField] private float maxSfxVolume = 1;

        // True when Awake() is called
        private bool isInitialised;


        private void Awake() {
            musicVolumeSlider.maxValue = maxMusicVolume;
            sfxVolumeSlider.maxValue = maxSfxVolume;

            musicVolumeSlider.value = Ctx.Deps.AudioController.MusicBusVolume;
            sfxVolumeSlider.value = Ctx.Deps.AudioController.SfxBusVolume;

            isInitialised = true;
        }

        public void OnMusicVolumeChanged() {
            if (!isInitialised) return;
            Ctx.Deps.AudioController.SetMusicVolume(musicVolumeSlider.value);
        }

        public void OnSfxVolumeChanged() {
            if (!isInitialised) return;
            Ctx.Deps.AudioController.SetSfxVolume(sfxVolumeSlider.value);
        }
    }
}