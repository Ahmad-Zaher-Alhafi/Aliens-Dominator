using Context;
using UnityEngine;

namespace Collectables {
    public class Supplies : MonoBehaviour {
        [SerializeField] private Constants.SuppliesTypes suppliesType;
        [Header("For Ammo supplies only")]
        [SerializeField] private int numOfAmmoInThePack;

        private void OnCollisionEnter(Collision other) {

            if (other.gameObject.tag != "Arrow") return;

            if (suppliesType == Constants.SuppliesTypes.ArrowUpgrade) {
                Ctx.Deps.EventsManager.OnGatheringSupplies();
            } else {
                Ctx.Deps.EventsManager.OnTakingAmmo(suppliesType, numOfAmmoInThePack);
            }

            Destroy(gameObject);
        }
    }
}