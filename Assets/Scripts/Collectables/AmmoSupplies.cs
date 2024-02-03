using Context;
using UnityEngine;

namespace Collectables {
    public class AmmoSupplies : Supplies {
        [SerializeField] private int numOfAmmoInThePack;

        protected override void OnCollected(Constants.SuppliesTypes suppliesType) {
            Ctx.Deps.EventsManager.TriggerAmmoSuppliesCollected(suppliesType, numOfAmmoInThePack);
        }
    }
}