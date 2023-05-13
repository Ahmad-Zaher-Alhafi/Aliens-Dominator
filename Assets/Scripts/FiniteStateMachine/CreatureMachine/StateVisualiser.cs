#if UNITY_EDITOR
using Creatures;
using FiniteStateMachine.States;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FiniteStateMachine.CreatureMachine {
    [ExecuteInEditMode]
    public class StateVisualiser : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI displayNameText;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color activeColor;
        [SerializeField] private StateType stateType;

        [SerializeField] private bool isFinal;
        public bool IsFinal => isFinal;

        public StateType StateType => stateType;

        [SerializeField] private Image image;
        // To prevent executing the code for the prefab asset itself
        private bool IsInstanceOfAsset => gameObject.scene.name != null && gameObject.scene.name != gameObject.name;
        public Bounds Bounds => image.sprite.bounds;
        private void Awake() {
            if (!IsInstanceOfAsset) return;
        }

        private void Update() {
            if (!IsInstanceOfAsset) return;
            Refresh();
        }

        private void Refresh() {
            name = stateType.ToString();
            displayNameText.text = stateType.ToString();

            if (Selection.activeObject == null) {
                image.color = defaultColor;
                return;
            }

            Creature selectedCreature = Selection.activeGameObject.GetComponent<Creature>();
            if (selectedCreature != null) {
                image.color = selectedCreature.CurrentState == StateType ? activeColor : defaultColor;
            } else {
                image.color = defaultColor;
            }
        }
    }
}
#endif