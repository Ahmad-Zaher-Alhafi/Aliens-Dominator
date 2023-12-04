using UnityEngine;
using UnityEngine.UI;

namespace ManagersAndControllers {
    public class WallManager : MonoBehaviour {
        public float Health = 100f;
        public Image HealthImage;
        public GameObject HealthbarCanvas;

        private Player.Player player;
        private GameController gameController;
        private float HealthFromBeginning;

        private void Awake() {
            HealthFromBeginning = Health;

            gameController = FindObjectOfType<GameController>();
            player = FindObjectOfType<Player.Player>();

//            HealthbarCanvas.transform.LookAt(ArcheryRig.transform);
        }

        public void ReduceHealth(float dmg) {
            Health -= dmg;

            HealthImage.fillAmount = Health / HealthFromBeginning;

            if (Health <= 0f) {
                player.enabled = false;
                Time.timeScale = 0;
            }
        }
    }
}