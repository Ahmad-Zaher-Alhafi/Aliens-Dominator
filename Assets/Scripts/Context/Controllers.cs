using Creatures;
using ManagersAndControllers;
using Multiplayer;
using UnityEngine;

namespace Context {
    public interface IControllers {
        public SupplyBalloonController SupplyBalloonController { get; }
        public CreatureSpawnController CreatureSpawnController { get; }
        public WaveController WaveController { get; }
        public EventsManager EventsManager { get; }
        public GameController GameController { get; }
        public AudioController AudioController { get; }
        public ObjectPoolController ObjectPoolController { get; }
        public Matchmaker Matchmaker { get; }
    }

    public class Controllers : MonoBehaviour, IControllers {
        [SerializeField] private SupplyBalloonController supplyBalloonController;
        public SupplyBalloonController SupplyBalloonController => supplyBalloonController;

        [SerializeField] private CreatureSpawnController creatureSpawnController;
        public CreatureSpawnController CreatureSpawnController => creatureSpawnController;

        [SerializeField] private WaveController waveController;
        public WaveController WaveController => waveController;

        [SerializeField] private EventsManager eventsManager;
        public EventsManager EventsManager => eventsManager;

        [SerializeField] private GameController gameController;
        public GameController GameController => gameController;

        [SerializeField] public AudioController audioController;
        public AudioController AudioController => audioController;

        [SerializeField] public ObjectPoolController objectPoolController;
        public ObjectPoolController ObjectPoolController => objectPoolController;

        [SerializeField] public Matchmaker matchmaker;
        public Matchmaker Matchmaker => matchmaker;

        private void Awake() {
            Ctx.ContextChanged(this);
        }
    }
}