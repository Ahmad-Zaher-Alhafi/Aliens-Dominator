using AmmoMagazines;
using Context;
using UnityEngine;

namespace Collectables {
    public class AmmoSupplies : Supplies {
        [SerializeField] private int numOfAmmoInThePack;
        [SerializeField] private Magazine.AmmoType ammoType;

        protected override void OnCollected(Constants.SuppliesTypes suppliesType) {
            base.OnCollected(suppliesType);
            Ctx.Deps.EventsManager.TriggerAmmoSuppliesCollected(ammoType, numOfAmmoInThePack);
        }
    }
}