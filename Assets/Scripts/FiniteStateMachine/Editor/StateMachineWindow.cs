#if UNITY_EDITOR
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FiniteStateMachine.Editor {
    public class StateMachineWindow : EditorWindow {
        [SerializeField] private GameObject stateVisualizersHolder;
        [SerializeField] private GameObject stateVisualizerPrefab;
        [SerializeField] private GameObject transitionVisualizerPrefab;


        private AsyncOperationHandle<GameObject> assetLoader;

        [MenuItem("Window/State Machine")]
        private static void ShowWindow() {
            EditorWindow window = GetWindow(typeof(StateMachineWindow));
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        private void OnGUI() {
            ShowObjectFields();
            GUILayout.Space(25);
            ShowCreateButtons();
        }


        private void ShowObjectFields() {
            GUILayout.Label("Holders");
            stateVisualizersHolder ??= GameObject.FindGameObjectWithTag("StatesHolder");

            CreateObjectFieldWithLabel(ref stateVisualizersHolder, "StateVisualisersHolder");

            if (stateVisualizerPrefab is null) {
                assetLoader = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/UI/State Machine/State Visualiser.prefab");
                stateVisualizerPrefab = assetLoader.Result;
            }

            GUILayout.Label("Prefabs");

            if (transitionVisualizerPrefab is null) {
                assetLoader = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/UI/State Machine/Transition Visualiser.prefab");
                transitionVisualizerPrefab = assetLoader.Result;
            }

            CreateObjectFieldWithLabel(ref stateVisualizerPrefab, "StateVisualiserPrefab");
            CreateObjectFieldWithLabel(ref transitionVisualizerPrefab, "TransitionVisualiserPrefab");
        }

        private void ShowCreateButtons() {
            if (GUILayout.Button("Create state")) {
                CreateState();
            } else if (GUILayout.Button("Create transition")) {
                CreateTransition();
            }
        }


        private void CreateState() {
            GameObject gameObject = Instantiate(stateVisualizerPrefab, stateVisualizersHolder.transform);
            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, "Assets/Prefabs/UI/State Machine/State Visualiser.prefab", InteractionMode.UserAction);
        }

        private void CreateTransition() {
            TransitionVisualiser transitionVisualiser = null;
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.TryGetComponent(out transitionVisualiser);
            }

            GameObject gameObject = Instantiate(transitionVisualizerPrefab, null);
            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, "Assets/Prefabs/UI/State Machine/Transition Visualiser.prefab", InteractionMode.UserAction);

            if (transitionVisualiser != null) {
                gameObject.GetComponent<TransitionVisualiser>().OriginStateType = transitionVisualiser.OriginStateType;
            }

            Selection.activeGameObject = gameObject;
        }

        private void CreateObjectFieldWithLabel(ref GameObject obj, string label) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            obj = EditorGUILayout.ObjectField(obj, typeof(Object), true) as GameObject;
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif