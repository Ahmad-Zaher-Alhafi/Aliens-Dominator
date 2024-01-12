using Unity.Netcode;

namespace Multiplayer {
    public struct SerializedNetworkString : INetworkSerializable {
        public string Value => value;
        private string value;

        public SerializedNetworkString(string value) {
            this.value = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref value);
        }
    }
}