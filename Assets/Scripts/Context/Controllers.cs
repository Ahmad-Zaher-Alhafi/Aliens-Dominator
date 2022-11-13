using Creatures;
using ManagersAndControllers;
using UnityEngine;

namespace Context {
    public interface IControllers {
        public SupplyBalloonController SupplyBalloonController { get; }
        public EventsManager EventsManager { get; }
    }

    public class Controllers : MonoBehaviour, IControllers {
        [SerializeField] private SupplyBalloonController supplyBalloonController;
        public SupplyBalloonController SupplyBalloonController => supplyBalloonController;
        
        [SerializeField] private EventsManager eventsManager;
        public EventsManager EventsManager => eventsManager;
        
        
        private void Awake() {
            Ctx.ContextChanged(this);
        }
    }
}