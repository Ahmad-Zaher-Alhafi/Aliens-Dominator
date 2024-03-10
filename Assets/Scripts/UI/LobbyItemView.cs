using Placeables;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace UI {
    public class LobbyItemView : MonoPlaceableObject {
        [SerializeField] private TextMeshProUGUI lobbyNameText;
        [SerializeField] private TextMeshProUGUI playersNumberText;
        [SerializeField] private TextMeshProUGUI lobbyCapacityText;


        private LobbyItemPlaceable lobbyItemPlaceable;

        public override void SetPlaceable(AddressablePlaceable placeable) {
            base.SetPlaceable(placeable);
            lobbyItemPlaceable = placeable as LobbyItemPlaceable;
            Init();
        }

        private void Init() {
            lobbyNameText.text = lobbyItemPlaceable.LobbyName;
            playersNumberText.text = lobbyItemPlaceable.PlayersNumber.ToString();
            lobbyCapacityText.text = lobbyItemPlaceable.LobbyCapacity.ToString();
        }

        public void SetLobby(Lobby lobby) {
            lobbyItemPlaceable.AssignLobby(lobby);
            Init();
        }

        public void SelectLobby() {
            lobbyItemPlaceable.OnSelected();
        }
    }
}