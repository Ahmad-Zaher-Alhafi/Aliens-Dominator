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
        [SerializeField] private UnityTransport unityTransport;
        public static Lobby ConnectedToLobby { get; private set; }
        private QueryResponse lobbies;
        private const string JoinCodeKey = "z";
        public static string PlayerId { get; private set; }

        public async void HostLobby(string playerName) {
            await Authenticate();

            ConnectedToLobby = await CreateLobby(playerName);
            NetworkManager.Singleton.StartHost();
        }

        public async void JoinLobby(string playerName) {
            await Authenticate();

            ConnectedToLobby = await GetLobbyToJoin(playerName);
            NetworkManager.Singleton.StartClient();
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

        private async Task<Lobby> GetLobbyToJoin(string playerName) {
            try {
                var player = new Unity.Services.Lobbies.Models.Player {
                    Data = new Dictionary<string, PlayerDataObject> {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                };
                QuickJoinLobbyOptions options = new QuickJoinLobbyOptions { Player = player };

                var lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);

                var allocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

                unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
                return lobby;
            } catch (Exception e) {
                Console.WriteLine("No lobby found!");
                return null;
            }
        }

        private async Task<Lobby> CreateLobby(string playerName) {
            try {
                const int maxPlayers = 100;

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

                var lobby = await Lobbies.Instance.CreateLobbyAsync("Set a better name", maxPlayers, options);

                StartCoroutine(HeartBeatLobby(lobby.Id, 15));

                unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
                return lobby;
            } catch (Exception e) {
                Debug.LogError("Could not create a lobby!");
                return null;
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
    }
}