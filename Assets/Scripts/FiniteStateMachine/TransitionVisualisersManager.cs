#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Creatures;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace FiniteStateMachine {
    [ExecuteInEditMode]
    public class TransitionVisualisersManager : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI creatureNameText;
        [SerializeField] private GameObject TransitionVisualiserPrefab;

        private List<TransitionVisualiser> transitions1;
        private List<TransitionVisualiser> transitions2;
        // To prevent executing the code for the prefab asset itself (Prevents error of prefab being out of prefab scene)
        private bool IsInstanceOfAsset => gameObject.scene.name != null && gameObject.scene.name != gameObject.name;
        private GameObject currentSelectedGameObject;


        private void Update() {
            if (!IsInstanceOfAsset) return;
            transitions1 = FindObjectsOfType<TransitionVisualiser>().ToList();
            transitions2 = transitions1.ToList();
            FixOverlappingTransitionsLines();

            // Display selected creature name
            if (Selection.activeObject != null && Selection.activeObject != currentSelectedGameObject) {
                currentSelectedGameObject = Selection.activeObject as GameObject;
                if (currentSelectedGameObject != null && currentSelectedGameObject.GetComponent<Creature>() != null) {
                    creatureNameText.SetText($"State machine for {currentSelectedGameObject.name}");
                }
            }
        }

        private void FixOverlappingTransitionsLines() {
            foreach (TransitionVisualiser transitionVis1 in transitions1) {
                //transitionVis1.Offset = Vector3.zero;
                foreach (TransitionVisualiser transitionVis2 in transitions2) {
                    // Check for any duplicated transitions that have to be removed
                    if (DestroyIfDuplicated(transitionVis1, transitionVis2)) return;

                    if (transitionVis2 == transitionVis1) continue;
                    if (transitionVis2.LastPoint != transitionVis1.FirstPoint || transitionVis2.FirstPoint != transitionVis1.LastPoint) continue;
                    transitionVis1.Offset = transitionVis1.transform.right * 2 + transitionVis1.transform.up * 1;
                    transitionVis2.Offset = transitionVis2.transform.right * -2 + transitionVis2.transform.up * -1;
                }
            }
        }

        /// <summary>
        /// Sometimes transitions are getting duplicated for some reason and i'm sick of deleting them manually so this function will do it
        /// </summary>
        /// <param name="transitionVis1"></param>
        /// <param name="transitionVis2"></param>
        /// <returns></returns>
        private bool DestroyIfDuplicated(TransitionVisualiser transitionVis1, TransitionVisualiser transitionVis2) {
            if (transitionVis1 == transitionVis2 || transitionVis1.transform.parent != transitionVis2.transform.parent || transitionVis1.name != transitionVis2.name) return false;
            transitions1.Remove(transitionVis1);
            PrefabUtility.UnpackAllInstancesOfPrefab(TransitionVisualiserPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            DestroyImmediate(transitionVis1.gameObject);
            return true;
        }
    }
}
#endif