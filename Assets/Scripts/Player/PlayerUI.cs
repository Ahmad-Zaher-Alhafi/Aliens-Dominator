using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class PlayerUI : NetworkBehaviour {
        /*[SerializeField] private TextMeshProUGUI playerNameText;

        private readonly NetworkVariable<SerializedString> networkPlayerName = new();

        public void SetName(SerializedString serializedString) {
            networkPlayerName.Value = serializedString;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetNameServerRPC(SerializedString serializedString) {
            networkPlayerName.Value = serializedString;
        }

        private void Update() {
            playerNameText.text = networkPlayerName.Value.Name;
        }

        public struct SerializedString : INetworkSerializable {
            public string Name => name;
            private string name;

            public SerializedString(string name) {
                this.name = name;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
                serializer.SerializeValue(ref name);
            }
        }*/
    }
}