using System;
using Context;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagersAndControllers {
    public class UIManager : MonoBehaviour {
        public Text GameoverText;
        public Text WonText;
        public Text CurrentStatus;
        public GameObject CurrentStatusParent;

        [SerializeField] private ArcheryRig archeryRig;
        [SerializeField] private GameObject UIMenue;
        [SerializeField] private GameObject playerBow;
        [SerializeField] private GameObject pauseBtn;
        [SerializeField] private PanelManager panelManager;
        [SerializeField] private TextMeshProUGUI resourcesNumText;
        [SerializeField] private CreatureSpawnController creatureSpawnController;


        public void SetupGameover(int score) {
            GameoverText.gameObject.SetActive(true);
            GameoverText.text = string.Format("Game over\nScore: {0}", score);
        }

        public void UpdateStatus(int wave, int maxWave, int level) {
            CurrentStatus.text = string.Format("Wave {0} of {1}\nCurrent level: {2}", wave, maxWave, level);
        }

        public void SetupWin(int score) {
            WonText.gameObject.SetActive(true);
            WonText.text = string.Format("You won!\nScore: {0}", score);
        }

        private void Start() {
            StartGame();
            gameObject.SetActive(false);
        }

        public void StartGame() {
            //UIMenue.SetActive(false);
            panelManager.CloseCurrent();
            playerBow.SetActive(true);
            archeryRig.WasGameStarted = true;

            CurrentStatusParent.SetActive(true);
        }

        public void Restart() {
            //SetTimeScale(1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void SetTimeScale(int timeScale) //to puase or unpause the agme
        {
            if (timeScale == 0) {

                //UIMenue.SetActive(true);
                panelManager.OpenPanel(panelManager.initiallyOpen);
                archeryRig.WasGameStarted = false;

                pauseBtn.SetActive(false);
                playerBow.SetActive(false);
            } else if (timeScale == 1) {
                pauseBtn.SetActive(true);
                //UIMenue.SetActive(false);
                panelManager.CloseCurrent();
                playerBow.SetActive(true);
                archeryRig.WasGameStarted = true;
            } else { }

            //Time.timeScale = timeScale;
        }

        public void UpdateResourcesNumText(float resources) {
            resourcesNumText.text = resources.ToString();
        }

        public void StartNextLevel() {
            Ctx.Deps.EventsManager.OnLevelStarts();
            creatureSpawnController.StartNextLevel();
        }
    }
}