using Context;
using UnityEngine;

namespace ManagersAndControllers {
    public class ResourcesProducer : MonoBehaviour {
        [SerializeField] private float numOfResourcesToProduceEachLevel;

        private GameController gameController;

        // Start is called before the first frame update
        private void Start() {
            gameController = FindObjectOfType<GameController>();
            Ctx.Deps.EventsManager.onLevelFinishs += ProduceResources;
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.onLevelFinishs -= ProduceResources;
        }


        private void ProduceResources() {
            gameController.UpdateResourcesCount(numOfResourcesToProduceEachLevel);
        }
    }
}