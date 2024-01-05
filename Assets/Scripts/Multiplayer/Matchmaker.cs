using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Multiplayer {
    public class Matchmaker : MonoBehaviour {
        [SerializeField] private UnityTransport unityTransport;
        private Lobby connectedToLobby;
        private QueryResponse lobbies;
        private const string JoinCodeKey = "z";
        private string playerId;


        public async void CreateOrJoinLobby() {
            await Authenticate();
            connectedToLobby = await QuickJoinLobby() ?? await CreateLobby();
        }

        private async Task Authenticate() {
            var options = new InitializationOptions();

            await UnityServices.InitializeAsync(options);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerId = AuthenticationService.Instance.PlayerId;
        }

        private async Task<Lobby> QuickJoinLobby() {
            try {
                var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

                var allocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

                SetTransformAsClient(allocation);

                NetworkManager.Singleton.StartClient();
                return lobby;
            } catch (Exception e) {
                Console.WriteLine("No lobby found!");
                return null;
            }

        }
        private void SetTransformAsClient(JoinAllocation allocation) {
            unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
        }

        private async Task<Lobby> CreateLobby() {
            try {
                const int maxPlayers = 100;

                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var options = new CreateLobbyOptions {
                    Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
                };

                var lobby = await Lobbies.Instance.CreateLobbyAsync("Set a better name", maxPlayers, options);

                StartCoroutine(HeartBeatLobby(lobby.Id, 15));

                unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                NetworkManager.Singleton.StartHost();

                return lobby;
            } catch (Exception e) {
                Console.WriteLine("Could not create a lobby!");
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