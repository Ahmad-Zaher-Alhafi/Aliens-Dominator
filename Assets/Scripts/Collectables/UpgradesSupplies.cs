using Context;

namespace Collectables {
    public class UpgradesSupplies : Supplies {
        protected override void OnCollected(Constants.SuppliesTypes suppliesType) {
            Ctx.Deps.EventsManager.TriggerUpgradesSuppliesCollected();
        }
    }
}