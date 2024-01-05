using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer {
    public class NetworkUI : NetworkBehaviour {
        [SerializeField] private Matchmaker matchmaker;

        [Header("Network stuff")]
        [SerializeField] private TextMeshProUGUI playerTypeText;
        [SerializeField] private TextMeshProUGUI pingText;
        [SerializeField] private TextMeshProUGUI numOfPlayersText;
        [SerializeField] private TextMeshProUGUI joiningText;
        [SerializeField] private GameObject joinButton;
        [SerializeField] private TMP_InputField playerNameField;

        [Header("Packet parameters")]
        [SerializeField] private TMP_InputField delay;
        [SerializeField] private TMP_InputField jitter;
        [SerializeField] private TMP_InputField dropRate;

        private readonly NetworkVariable<int> networkNumOfPlayers = new();

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            joiningText.gameObject.SetActive(false);
            pingText.gameObject.SetActive(true);
            numOfPlayersText.gameObject.SetActive(true);
            playerTypeText.gameObject.SetActive(true);

            delay.gameObject.SetActive(false);
            jitter.gameObject.SetActive(false);
            dropRate.gameObject.SetActive(false);

            playerTypeText.text = IsServer ? "Player is: Server" : "Player is: Client";

            if (IsServer) {
                networkNumOfPlayers.Value += 1;
            } else {
                OnPlayerSpawnedServerRPC();
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (IsServer) {
                networkNumOfPlayers.Value -= 1;
            } else {
                OnPlayerDespawnServerRPC();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnPlayerSpawnedServerRPC() {
            networkNumOfPlayers.Value += 1;
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnPlayerDespawnServerRPC() {
            networkNumOfPlayers.Value -= 1;
        }

        private void Update() {
            if (!IsSpawned) return;
            numOfPlayersText.text = $"Players: {networkNumOfPlayers.Value.ToString()}";
            if (IsClient) {
                pingText.text = $"Ping:  + {NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(0)} ms";
            }
        }

        public void JoinClicked() {
            joinButton.SetActive(false);
            joiningText.gameObject.SetActive(true);
            matchmaker.CreateOrJoinLobby();
        }

        public void SetPlayerName() {
            // Set the player name
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
            matchmaker.SetPacketParameters(delayValue, jitterValue, dropRateValue);
        }
    }
}