using Context;
using Placeables;
using Unity.Services.Lobbies.Models;

namespace UI {
    public class LobbyItemPlaceable : AddressablePlaceable {
        public string LobbyName => lobby.Name;
        public int PlayersNumber => lobby.Players.Count;
        public int LobbyCapacity => lobby.MaxPlayers;
        public string LobbyId => lobby.Id;


        private Lobby lobby;
        public bool IsSelected { get; private set; }

        public LobbyItemPlaceable(Lobby lobby) : base("Assets/Prefabs/UI/Menu Components/Lobby Item View.prefab") {
            AssignLobby(lobby);
        }

        public void AssignLobby(Lobby lobby) {
            this.lobby = lobby;
        }

        public void OnSelected() {
            DeselectAllLobbyItems();
            IsSelected = true;
        }

        private void DeselectAllLobbyItems() {
            foreach (LobbyItemPlaceable lobbyItemPlaceable in Ctx.Deps.PlaceablesController.GetPlaceablesOfType<LobbyItemPlaceable>()) {
                lobbyItemPlaceable.IsSelected = false;
            }
        }
    }
}