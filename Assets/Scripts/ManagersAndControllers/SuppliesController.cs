using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace ManagersAndControllers {
    public class SuppliesController : NetworkBehaviour {
        public enum SuppliesTypes {
            Construction,
            RocketsAmmo,
            BulletsAmmo
        }

        [SerializeField] private int constructionSuppliesAmountOnStart = 500;

        private Dictionary<SuppliesTypes, int> supplies = new() {
            { SuppliesTypes.Construction, 0 },
            { SuppliesTypes.BulletsAmmo, 0 },
            { SuppliesTypes.RocketsAmmo, 0 }
        };

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            PlusSupplies(SuppliesTypes.Construction, constructionSuppliesAmountOnStart);
        }

        public void PlusSupplies(SuppliesTypes type, int amount) {
            if (!IsServer) throw new ArgumentException("Supplies are managed by server only");
            supplies[type] += amount;
            OnSuppliesAmountChangedClientRPC(new SerializedNetworkSuppliesDictionary(supplies));
        }

        private void MinusSupplies(SuppliesTypes type, int amount) {
            if (!IsServer) throw new ArgumentException("Supplies are managed by server only");
            supplies[type] -= amount;
            OnSuppliesAmountChangedClientRPC(new SerializedNetworkSuppliesDictionary(supplies));
        }

        public bool HasEnoughSupplies(SuppliesTypes type, int wantedAmount) {
            return supplies[type] >= wantedAmount;
        }

        public bool TryConsumeSupplies(SuppliesTypes type, int amount) {
            if (!IsServer) throw new ArgumentException("Supplies are managed by server only");
            if (supplies[type] < amount) return false;

            MinusSupplies(type, amount);
            return true;
        }

        [ClientRpc]
        private void OnSuppliesAmountChangedClientRPC(SerializedNetworkSuppliesDictionary newSupplies) {
            if (IsServer) return;
            supplies = newSupplies.ToDictionary();
        }

        private struct SerializedNetworkSuppliesDictionary : INetworkSerializable {
            private SuppliesTypes[] keys;
            private int[] values;

            public SerializedNetworkSuppliesDictionary(Dictionary<SuppliesTypes, int> dictionary) {
                keys = dictionary.Keys.ToArray();
                values = dictionary.Values.ToArray();
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
                serializer.SerializeValue(ref keys);
                serializer.SerializeValue(ref values);
            }

            public Dictionary<SuppliesTypes, int> ToDictionary() {
                var dictionary = new Dictionary<SuppliesTypes, int>();
                for (int i = 0; i < keys.Length; i++) {
                    dictionary.Add(keys[i], values[i]);
                }
                return dictionary;
            }
        }
    }
}