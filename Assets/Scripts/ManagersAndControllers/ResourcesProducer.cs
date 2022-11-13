using Context;
using UnityEngine;

namespace ManagersAndControllers {
    public class ResourcesProducer : MonoBehaviour {
        [SerializeField] private float numOfResourcesToProduceEachLevel;

        private GameHandler gameHandler;

        // Start is called before the first frame update
        private void Start() {
            gameHandler = FindObjectOfType<GameHandler>();
            Ctx.Deps.EventsManager.onLevelFinishs += ProduceResources;
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.onLevelFinishs -= ProduceResources;
        }


        private void ProduceResources() {
            gameHandler.UpdateResourcesCount(numOfResourcesToProduceEachLevel);
        }
    }
}