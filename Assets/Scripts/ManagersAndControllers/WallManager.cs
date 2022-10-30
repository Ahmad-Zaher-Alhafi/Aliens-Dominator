using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace ManagersAndControllers {
    public class WallManager : MonoBehaviour {
        public float Health = 100f;
        public Image HealthImage;
        public GameObject HealthbarCanvas;

        public List<AudioClip> WallHitSounds = new();
        private ArcheryRig ArcheryRig;
        private AudioSource AudioSource;
        private GameHandler GameHandler;
        private float HealthFromBeginning;

        private void Awake() {
            AudioSource = GetComponent<AudioSource>();

            HealthFromBeginning = Health;

            GameHandler = FindObjectOfType<GameHandler>();
            ArcheryRig = FindObjectOfType<ArcheryRig>();

//            HealthbarCanvas.transform.LookAt(ArcheryRig.transform);
        }

        public void ReduceHealth(float dmg) {
            Health -= dmg;

            AudioSource.PlayOneShot(WallHitSounds[Random.Range(0, WallHitSounds.Count)]);

            HealthImage.fillAmount = Health / HealthFromBeginning;

            if (Health <= 0f) {
                ArcheryRig.enabled = false;
                Time.timeScale = 0;

                GameHandler.GameOver(GameHandler.Spawner.Score);
            }
        }
    }
}