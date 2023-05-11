using Creatures;
using ManagersAndControllers;
using UnityEngine;

namespace Context {
    public interface IControllers {
        public SupplyBalloonController SupplyBalloonController { get; }
        public CreatureSpawnController CreatureSpawnController { get; }
        public EventsManager EventsManager { get; }
        public GameController GameController { get; }
    }

    public class Controllers : MonoBehaviour, IControllers {
        [SerializeField] private SupplyBalloonController supplyBalloonController;
        public SupplyBalloonController SupplyBalloonController => supplyBalloonController;

        [SerializeField] private CreatureSpawnController creatureSpawnController;
        public CreatureSpawnController CreatureSpawnController => creatureSpawnController;

        [SerializeField] private EventsManager eventsManager;
        public EventsManager EventsManager => eventsManager;
        
        [SerializeField] private GameController gameController;
        public GameController GameController => gameController;


        private void Awake() {
            Ctx.ContextChanged(this);
        }
    }
}