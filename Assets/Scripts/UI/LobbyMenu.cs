using System.Collections.Generic;
using System.Linq;
using Context;
using Placeables;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace UI {
    public class LobbyMenu : MonoBehaviour {
        [SerializeField] private Transform lobbiesHolder;

        private void OnEnable() {
            CreateLobbyViews();
        }

        private async void CreateLobbyViews() {
            List<Lobby> availableLobbies = await Ctx.Deps.Matchmaker.GetAvailableLobbies();
            IEnumerator<LobbyItemView> enumerator = lobbiesHolder.GetComponentsInChildren<LobbyItemView>().ToList().GetEnumerator();

            int counter = 0;
            // Reuse the current created views
            while (enumerator.MoveNext()) {
                Lobby lobby = availableLobbies[counter];
                enumerator.Current.SetLobby(lobby);
                counter++;
            }

            // Create the rest
            for (int i = 0; i < availableLobbies.Count - counter; i++) {
                Ctx.Deps.PlaceablesController.Place<MonoPlaceableObject>(new LobbyItemPlaceable(availableLobbies[i]), lobbiesHolder);
            }

            enumerator.Dispose();
        }
    }
}