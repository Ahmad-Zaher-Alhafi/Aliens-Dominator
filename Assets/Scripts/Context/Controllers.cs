using Creatures;
using UnityEngine;

namespace Context {

    public interface IControllers {
        public SupplyBalloonController SupplyBalloonController { get; }
    }

    public class Controllers : MonoBehaviour, IControllers {
        private void Awake() {
            Ctx.ContextChanged(this);
        }

        [SerializeField] private SupplyBalloonController supplyBalloonController;
        public SupplyBalloonController SupplyBalloonController => supplyBalloonController;

    }
}