using System.Linq;
using Context;
using Multiplayer;
using Placeables;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI {
    public class MenuUI : NetworkBehaviour {
        [SerializeField] private TMP_InputField playerNameField;

        [Space]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject lobbyMenu;
        [SerializeField] private GameObject mainMenuBottomButtons;
        [SerializeField] private GameObject lobbyMenuBottomButtons;

        [Space]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;

        [Space]
        [SerializeField] private NetworkStatus networkStatus;

        private string PlayerName => !string.IsNullOrEmpty(playerNameField.text) ? playerNameField.text : $"Player: {networkStatus.NumOfPlayers}";


        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            gameObject.SetActive(false);
        }

        private void Update() {
            hostButton.interactable = Ctx.Deps.Matchmaker.Status == Matchmaker.LobbyStatus.None;
            joinButton.interactable = Ctx.Deps.Matchmaker.Status == Matchmaker.LobbyStatus.None &&
                                      Ctx.Deps.PlaceablesController.GetPlaceablesOfType<LobbyItemPlaceable>().Any(placeable => placeable.IsSelected);
        }

        public void ShowMainMenuClicked() {
            DeactivateAllMenus();
            mainMenu.SetActive(true);
            lobbyMenuBottomButtons.SetActive(false);
            mainMenuBottomButtons.SetActive(true);
        }

        public void ShowLobbyMenuClicked() {
            DeactivateAllMenus();
            lobbyMenu.SetActive(true);
            mainMenuBottomButtons.SetActive(false);
            lobbyMenuBottomButtons.SetActive(true);
        }

        private void DeactivateAllMenus() {
            mainMenu.SetActiveWithCheck(false);
            lobbyMenu.SetActiveWithCheck(false);
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
    }
}