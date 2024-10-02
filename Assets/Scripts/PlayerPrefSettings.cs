using UnityEngine;
public static class PlayerPrefSettings {
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    public static void SetMusicVolume(float volume) {
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public static float? GetMusicVolume() {
        float value = PlayerPrefs.GetFloat(MusicVolumeKey, -1);
        if (Mathf.Approximately(value, -1)) return null;
        return value;
    }

    public static void SetSfxVolume(float volume) {
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
    }

    public static float? GetSfxVolume() {
        float value = PlayerPrefs.GetFloat(SfxVolumeKey, -1);
        if (Mathf.Approximately(value, -1)) return null;
        return value;
    }
}