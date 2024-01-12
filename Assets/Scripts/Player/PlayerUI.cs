using System.Linq;
using Multiplayer;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class PlayerUI : NetworkBehaviour {
        [SerializeField] private TextMeshProUGUI playerNameText;

        private readonly NetworkVariable<SerializedNetworkString> networkName = new(new SerializedNetworkString(string.Empty));

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsOwner) return;

            string playerName = Matchmaker.ConnectedToLobby.Players.Single(player => player.Id == Matchmaker.PlayerId).Data["PlayerName"].Value;

            if (IsServer) {
                SetName(playerName);
                networkName.Value = new SerializedNetworkString(playerName);
            } else {
                SetPlayerNameServerRPC(new SerializedNetworkString(playerName));
            }
        }

        private void Update() {
            SetName(networkName.Value.Value);
        }

        private void SetName(string name) {
            playerNameText.text = name;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerNameServerRPC(SerializedNetworkString nameHolder) {
            networkName.Value = nameHolder;
        }
    }
}