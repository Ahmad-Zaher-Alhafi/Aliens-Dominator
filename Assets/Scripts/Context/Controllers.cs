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
        public PlaceablesController PlaceablesController { get; }
        public SupplyPlanesController SupplyPlanesController { get; }
        public CameraController CameraController { get; }
        public SuppliesController SuppliesController { get; }
        public ConstructionController ConstructionController { get; }
        public PointsController PointsController { get; }
        public InputActions InputActions { get; }
        public AbilitiesController AbilitiesController { get; }
        public InputController InputController { get; }
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

        [SerializeField] private AudioController audioController;
        public AudioController AudioController => audioController;

        [SerializeField] private ObjectPoolController objectPoolController;
        public ObjectPoolController ObjectPoolController => objectPoolController;

        [SerializeField] private Matchmaker matchmaker;
        public Matchmaker Matchmaker => matchmaker;

        [SerializeField] private PlaceablesController placeablesController;
        public PlaceablesController PlaceablesController => placeablesController;

        [SerializeField] private SupplyPlanesController supplyPlanesController;
        public SupplyPlanesController SupplyPlanesController => supplyPlanesController;

        [SerializeField] private CameraController cameraController;
        public CameraController CameraController => cameraController;

        [SerializeField] private SuppliesController suppliesController;
        public SuppliesController SuppliesController => suppliesController;

        [SerializeField] private ConstructionController constructionController;
        public ConstructionController ConstructionController => constructionController;

        [SerializeField] private PointsController pointsController;
        public PointsController PointsController => pointsController;

        [SerializeField] private AbilitiesController abilitiesController;
        public AbilitiesController AbilitiesController => abilitiesController;

        [SerializeField] private InputController inputController;
        public InputController InputController => inputController;

        public InputActions InputActions { get; private set; }

        private void Awake() {
            InputActions = new InputActions();
            InputActions.Enable();
            Ctx.ContextChanged(this);
        }
    }
}