using UnityEngine;

namespace ManagersAndControllers {
    public class ResourcesProducer : MonoBehaviour {
        [SerializeField] private float numOfResourcesToProduceEachLevel;

        private GameHandler gameHandler;

        // Start is called before the first frame update
        private void Start() {
            gameHandler = FindObjectOfType<GameHandler>();
            EventsManager.onLevelFinishs += ProduceResources;
        }

        private void OnDestroy() {
            EventsManager.onLevelFinishs -= ProduceResources;
        }


        private void ProduceResources() {
            gameHandler.UpdateResourcesCount(numOfResourcesToProduceEachLevel);
        }
    }
}