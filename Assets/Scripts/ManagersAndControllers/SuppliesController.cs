using System.Collections.Generic;
using UnityEngine;

namespace ManagersAndControllers {
    public class SuppliesController : MonoBehaviour {
        public enum SuppliesTypes {
            Construction,
            RocketsAmmo,
            BulletsAmmo
        }

        private readonly Dictionary<SuppliesTypes, int> supplies = new() {
            { SuppliesTypes.Construction, 0 },
            { SuppliesTypes.BulletsAmmo, 0 },
            { SuppliesTypes.RocketsAmmo, 0 }
        };

        public void PlusSupplies(SuppliesTypes type, int amount) {
            supplies[type] += amount;
        }

        public void MinusSupplies(SuppliesTypes type, int amount) {
            supplies[type] -= amount;
        }

        public bool HasEnoughSupplies(SuppliesTypes type, int wantedAmount) {
            return supplies[type] >= wantedAmount;
        }

        public bool TryConsumeSupplies(SuppliesTypes type, int amount) {
            if (supplies[type] < amount) return false;

            MinusSupplies(type, amount);
            return true;
        }
    }
}