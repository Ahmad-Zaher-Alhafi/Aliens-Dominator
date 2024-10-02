using System.Collections;
using System.Linq;
using Context;
using Multiplayer;
using Placeables;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI {
    public class MenuUI : MonoBehaviour {
        [SerializeField] private GameObject playerNameFieldHolder;
        [SerializeField] private TMP_InputField playerNameField;

        [Space]
        [SerializeField] private GameObject menuHolder;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject lobbyMenu;
        [SerializeField] private GameObject settingsMenu;

        [Header("Main menu buttons")]
        [SerializeField] private GameObject mainMenuBottomButtons;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button findMatchButton;
        [SerializeField] private Button quitMatchButton;

        [Space]
        [SerializeField] private GameObject lobbyMenuBottomButtons;
        [SerializeField] private GameObject settingsMenuBottomButtons;

        [Space]
        [SerializeField] private Button joinButton;

        [Space]
        [SerializeField] private NetworkStatus networkStatus;

        private string PlayerName => !string.IsNullOrEmpty(playerNameField.text) ? playerNameField.text : $"Player: {networkStatus.NumOfPlayers}";

        private void Awake() {
            UpdateManuVisibilityState(true);
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork += OnPlayerSpawnedOnNetwork;
        }

        private void OnPlayerSpawnedOnNetwork(Player.Player player) {
            if (!player.IsOwner) return;

            menuHolder.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update() {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0) && !menuHolder.activeSelf && EventSystem.current.currentSelectedGameObject == null && Ctx.Deps.GameController.IsSpawned) {
                Cursor.lockState = CursorLockMode.Locked;
            } else if (Input.GetMouseButtonDown(1)) {
                Cursor.lockState = CursorLockMode.None;
            }
#endif

            if (Input.GetKeyDown(KeyCode.Escape)) {
                MenuButtonClicked();
            }

            hostButton.interactable = Ctx.Deps.Matchmaker.Status == Matchmaker.LobbyStatus.None;
            joinButton.interactable = Ctx.Deps.Matchmaker.Status == Matchmaker.LobbyStatus.None &&
                                      Ctx.Deps.PlaceablesController.GetPlaceablesOfType<LobbyItemPlaceable>().Any(placeable => placeable.IsSelected);

            hostButton.gameObject.SetActiveWithCheck(Ctx.Deps.Matchmaker.Status != Matchmaker.LobbyStatus.InGame);
            findMatchButton.gameObject.SetActiveWithCheck(Ctx.Deps.Matchmaker.Status != Matchmaker.LobbyStatus.InGame);
            quitMatchButton.gameObject.SetActiveWithCheck(Ctx.Deps.Matchmaker.Status == Matchmaker.LobbyStatus.InGame);
        }

        public void MenuButtonClicked() => UpdateManuVisibilityState(!menuHolder.activeSelf);

        private void UpdateManuVisibilityState(bool showMenu) {
            menuHolder.SetActive(showMenu);
            Cursor.lockState = showMenu || !Ctx.Deps.GameController.IsSpawned ? CursorLockMode.None : CursorLockMode.Locked;
            if (showMenu) {
                ShowMainMenuClicked();
            }
        }

        public void ShowMainMenuClicked() {
            DeactivateAllMenus();
            mainMenu.SetActive(true);
            lobbyMenuBottomButtons.SetActive(false);
            mainMenuBottomButtons.SetActive(true);
            playerNameFieldHolder.gameObject.SetActive(!Ctx.Deps.GameController.IsSpawned);
        }

        public void ShowLobbyMenuClicked() {
            DeactivateAllMenus();
            lobbyMenu.SetActive(true);
            lobbyMenuBottomButtons.SetActive(true);
        }

        public void ShowSettingsMenuClicked() {
            DeactivateAllMenus();
            settingsMenu.SetActive(true);
            settingsMenuBottomButtons.SetActive(true);
        }

        private void DeactivateAllMenus() {
            mainMenu.SetActive(false);
            lobbyMenu.SetActive(false);
            settingsMenu.SetActive(false);

            mainMenuBottomButtons.SetActive(false);
            lobbyMenuBottomButtons.SetActive(false);
            settingsMenuBottomButtons.SetActive(false);
        }

        public async void HostLobbyClicked() {
            networkStatus.ShowHostingStatus();
            await Ctx.Deps.Matchmaker.HostLobby(PlayerName);
        }

        public async void JoinLobbyClicked() {
            string lobbyId = Ctx.Deps.PlaceablesController.GetPlaceablesOfType<LobbyItemPlaceable>().SingleOrDefault(placeable => placeable.IsSelected)?.LobbyId;
            if (string.IsNullOrEmpty(lobbyId)) {
                Debug.LogError("No lobby selected to join!");
                return;
            }

            networkStatus.ShowJoiningStatus();
            await Ctx.Deps.Matchmaker.JoinLobby(lobbyId, PlayerName);
        }

        public void QuitMatchClicked() {
            Ctx.Deps.GameController.QuitMatch();
            StartCoroutine(ShowMainMenuDelayed());
        }

        private IEnumerator ShowMainMenuDelayed() {
            yield return new WaitForEndOfFrame();
            ShowMainMenuClicked();
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork -= OnPlayerSpawnedOnNetwork;
        }
    }
}