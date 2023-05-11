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

        private List<TransitionVisualiser> transitions;
        // To prevent executing the code for the prefab asset itself (Prevents error of prefab being out of prefab scene)
        private bool IsInstanceOfAsset => gameObject.scene.name != null && gameObject.scene.name != gameObject.name;
        private GameObject currentSelectedGameObject;

        private void OnEnable() {
            if (!IsInstanceOfAsset) return;
            transitions = FindObjectsOfType<TransitionVisualiser>().ToList();
        }

        private void Update() {
            if (!IsInstanceOfAsset) return;
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
            foreach (TransitionVisualiser transitionVis1 in transitions) {
                //transitionVis1.Offset = Vector3.zero;
                foreach (TransitionVisualiser transitionVis2 in transitions) {
                    if (DestroyIfDuplicated(transitionVis1, transitionVis2)) continue;

                    if (transitionVis2 == transitionVis1) continue;
                    if (transitionVis2.LastPoint != transitionVis1.FirstPoint || transitionVis2.FirstPoint != transitionVis1.LastPoint) continue;
                    transitionVis1.Offset = transitionVis1.transform.right * 2 + transitionVis1.transform.up * 1;
                    transitionVis2.Offset = transitionVis2.transform.right * -2 + transitionVis2.transform.up * -1;
                }
            }
        }

        /// <summary>
        /// Sometimes transitions are getting duplicated for some reason and i'm sick of deleting them manually
        /// </summary>
        /// <param name="transitionVis1"></param>
        /// <param name="transitionVis2"></param>
        /// <returns></returns>
        private bool DestroyIfDuplicated(TransitionVisualiser transitionVis1, TransitionVisualiser transitionVis2) {
            if (transitionVis1 == transitionVis2 || transitionVis1.transform.parent != transitionVis2.transform.parent || transitionVis1.name != transitionVis2.name) return false;
            transitions.Remove(transitionVis2);
            DestroyImmediate(transitionVis2.gameObject);
            return true;
        }
    }
}
#endif