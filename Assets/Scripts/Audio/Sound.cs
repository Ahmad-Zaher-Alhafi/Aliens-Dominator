using UnityEngine;

namespace Audio {
    public class Sound : MonoBehaviour {
        [SerializeField] private AudioClip audioClip;
        public AudioClip AudioClip => audioClip;

        [SerializeField]
        private float volume = 1;
        public float Volume => volume;
    }
}