using Creatures;
using ManagersAndControllers;
using UnityEngine;

namespace Context {
    public interface IControllers {
        public SupplyBalloonController SupplyBalloonController { get; }
        public CreatureSpawnController CreatureSpawnController { get; }
        public EventsManager EventsManager { get; }
        public GameController GameController { get; }
        public AudioController AudioController { get; }
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

        [SerializeField]
        public AudioController audioController;
        public AudioController AudioController => audioController;

        private void Awake() {
            Ctx.ContextChanged(this);
        }
    }
}