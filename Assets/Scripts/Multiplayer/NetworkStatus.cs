using System;
using Context;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer {
    public class NetworkStatus : NetworkBehaviour {
        [Header("Network stuff")]
        [SerializeField] private TextMeshProUGUI playerTypeText;
        [SerializeField] private TextMeshProUGUI pingText;
        [SerializeField] private TextMeshProUGUI numOfPlayersText;
        [SerializeField] private TextMeshProUGUI joinHostText;

        [Header("Packet parameters")]
        [SerializeField] private TMP_InputField delay;
        [SerializeField] private TMP_InputField jitter;
        [SerializeField] private TMP_InputField dropRate;

        private readonly NetworkVariable<int> networkNumOfPlayers = new();
        public int NumOfPlayers => networkNumOfPlayers.Value;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            joinHostText.gameObject.SetActive(false);
            pingText.gameObject.SetActive(true);
            numOfPlayersText.gameObject.SetActive(true);
            playerTypeText.gameObject.SetActive(true);

            delay.gameObject.SetActive(false);
            jitter.gameObject.SetActive(false);
            dropRate.gameObject.SetActive(false);

            playerTypeText.text = IsServer ? "Player is: Server" : "Player is: Client";

            if (IsServer) {
                networkNumOfPlayers.Value = 1;
                Ctx.Deps.EventsManager.PlayerDespawnedFromNetwork += OnPlayerDespawnedFromNetwork;
                Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork += OnPlayerSpawnedOnNetwork;
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();

            pingText.gameObject.SetActive(false);
            numOfPlayersText.gameObject.SetActive(false);
            playerTypeText.gameObject.SetActive(false);

            if (IsServer) {
                Ctx.Deps.EventsManager.PlayerDespawnedFromNetwork -= OnPlayerDespawnedFromNetwork;
                Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork -= OnPlayerSpawnedOnNetwork;
            }
        }

        private void OnPlayerSpawnedOnNetwork(Player.Player player) {
            networkNumOfPlayers.Value += 1;
        }

        private void OnPlayerDespawnedFromNetwork(Player.Player player) {
            if (Ctx.Deps.Matchmaker.Status == Matchmaker.LobbyStatus.None || player.IsOwner) return;
            networkNumOfPlayers.Value -= 1;
        }

        private void Update() {
            if (!IsSpawned) return;
            numOfPlayersText.text = $"Players: {NumOfPlayers.ToString()}";

            if (IsClient) {
                pingText.text = $"Ping:  + {NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(0)} ms";
            }
        }

        public void ShowHostingStatus() {
            joinHostText.text = "Hosting...";
            joinHostText.gameObject.SetActive(true);
        }

        public void ShowJoiningStatus() {
            joinHostText.text = "Joining...";
            joinHostText.gameObject.SetActive(true);
        }

        public void SetPacketParameters() {
            if (delay.text == string.Empty) {
                delay.text = "0";
            }

            if (jitter.text == string.Empty) {
                jitter.text = "0";
            }

            if (dropRate.text == string.Empty) {
                dropRate.text = "0";
            }

            int delayValue = int.Parse(delay.text);
            int jitterValue = int.Parse(jitter.text);
            int dropRateValue = int.Parse(dropRate.text);

            Ctx.Deps.Matchmaker.SetPacketParameters(delayValue, jitterValue, dropRateValue);
        }
    }
}