using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParrelSync;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

namespace Multiplayer {
    public class Matchmaker : MonoBehaviour {
        public enum LobbyStatus {
            None,
            Hosting,
            Joining,
            InGame
        }

        [SerializeField] private UnityTransport unityTransport;
        public static Lobby ConnectedToLobby { get; private set; }
        private QueryResponse lobbies;
        private const string JoinCodeKey = "z";
        public static string PlayerId { get; private set; }
        public LobbyStatus Status { get; private set; }

        private async void Awake() {
            await Authenticate();
        }

        private async Task Authenticate() {
            var options = new InitializationOptions();

#if UNITY_EDITOR
            // To prevent the lobby from thinking that the two editors are related to same user with same id
            // Only of the ParrelSync package testing
            options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

            await UnityServices.InitializeAsync(options);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            PlayerId = AuthenticationService.Instance.PlayerId;
        }

        public async Task JoinLobby(string lobbyId, string playerName) {
            try {
                if (UnityServices.State == ServicesInitializationState.Uninitialized) return;
                Status = LobbyStatus.Joining;

                var player = new Unity.Services.Lobbies.Models.Player {
                    Data = new Dictionary<string, PlayerDataObject> {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                };
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions { Player = player };

                ConnectedToLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);

                var allocation = await RelayService.Instance.JoinAllocationAsync(ConnectedToLobby.Data[JoinCodeKey].Value);

                unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

                NetworkManager.Singleton.StartClient();
                Status = LobbyStatus.InGame;
            } catch (Exception e) {
                Console.WriteLine("No lobby found!");
                Status = LobbyStatus.None;
            }
        }

        public async Task HostLobby(string playerName) {
            try {
                if (UnityServices.State == ServicesInitializationState.Uninitialized) return;
                Status = LobbyStatus.Hosting;

                const int maxPlayers = 8;

                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var player = new Unity.Services.Lobbies.Models.Player {
                    Data = new Dictionary<string, PlayerDataObject> {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                };

                var options = new CreateLobbyOptions {
                    Player = player,
                    Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
                };

                ConnectedToLobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", maxPlayers, options);

                StartCoroutine(HeartBeatLobby(ConnectedToLobby.Id, 15));

                unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                NetworkManager.Singleton.StartHost();
                Status = LobbyStatus.InGame;
            } catch (Exception e) {
                Debug.LogError("Could not create a lobby!");
                Status = LobbyStatus.None;
            }
        }

        private IEnumerator HeartBeatLobby(string lobbyId, float shutdownDelay) {
            var delay = new WaitForSecondsRealtime(shutdownDelay);

            while (true) {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }

        public void SetPacketParameters(int delay, int jitter, int dropRate) {
            unityTransport.SetDebugSimulatorParameters(delay, jitter, dropRate);
        }

        public async Task<List<Lobby>> GetAvailableLobbies() {
            if (UnityServices.State == ServicesInitializationState.Uninitialized) return null;

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            return queryResponse.Results;
        }
    }
}