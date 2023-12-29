using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer {
    public struct SerializedNetworkVector3List : INetworkSerializable {
        public IEnumerable<Vector3> Objects => objects;
        private Vector3[] objects;

        public SerializedNetworkVector3List(Vector3[] objects) {
            this.objects = objects;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref objects);
        }
    }
}