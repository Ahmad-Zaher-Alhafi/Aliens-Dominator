using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine.States;
using Unity.Mathematics;
using UnityEngine;

namespace FiniteStateMachine.CreatureMachine {
    [ExecuteInEditMode]
    public class TransitionVisualiser : MonoBehaviour {
        public Vector3 Offset { get; set; }
        [SerializeField] private Type type;

        [Header("States")]
        [SerializeField] private StateType originStateType;
        [SerializeField] private StateType destinationStateType;
        public StateType OriginStateType {
            get => originStateType;
            set => originStateType = value;
        }

        public StateType DestinationStateType => destinationStateType;
        [SerializeField] private bool canInterrupts;
        public bool CanInterrupts => canInterrupts;

        [Header("Colors")]
        [SerializeField] private Material transitionVisualiserMaterial;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color activeColor;

        private StateVisualiser originStateVisualizer;
        private StateVisualiser destinationStateVisualizer;

        [SerializeField] private LineRenderer lineRenderer;
        private readonly float zPosition = 1;

        private List<StateVisualiser> stateVisualisers = new();
        // To prevent executing the code for the prefab asset itself (Prevents error of prefab being out of prefab scene)
        private bool IsInstanceOfAsset => gameObject.scene.name != null && gameObject.scene.name != gameObject.name;
        private bool isAdjusted;
        public Vector3 FirstPoint => lineRenderer.GetPosition(0);
        public Vector3 LastPoint => lineRenderer.GetPosition(lineRenderer.positionCount - 1);

        private void Awake() {
            if (!IsInstanceOfAsset) return;
            transform.position = new Vector3(transform.position.x, transform.position.y, zPosition);
        }

        private void Update() {
            if (!IsInstanceOfAsset) return;

            name = $"Transition {originStateType} => {destinationStateType}";
            LinkWithStates();
            AdjustPosition();
        }

        private void LinkWithStates() {
            stateVisualisers = FindObjectsOfType<StateVisualiser>().ToList();
            if (stateVisualisers.Count == 0) return;

            if (originStateVisualizer is null || originStateType != originStateVisualizer.StateType) {
                originStateVisualizer = stateVisualisers.FirstOrDefault(state => state.StateType == originStateType);
                if (originStateVisualizer != null) {
                    transform.position = originStateVisualizer.transform.position;
                    transform.SetParent(originStateVisualizer.transform);
                }
            }

            if (destinationStateVisualizer is null || destinationStateType != destinationStateVisualizer.StateType) {
                destinationStateVisualizer = stateVisualisers.FirstOrDefault(state => state.StateType == destinationStateType);
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = quaternion.identity;
            transform.localScale = Vector3.one * 2;
        }

        /// <summary>
        /// Make sure that the transition line is linked and moving with the states
        /// </summary>
        private void AdjustPosition() {
            lineRenderer.positionCount = 2;

            if (originStateVisualizer != null) {
                Vector3 originPosition = originStateVisualizer.transform.position + transform.forward * 1 /*- originStateVisualizer.Bounds.extents*/ + Offset;
                lineRenderer.SetPosition(0, originPosition);
            }

            if (destinationStateVisualizer != null) {
                Vector3 destinationPosition = destinationStateVisualizer.transform.position + transform.forward * 1 /*- destinationStateVisualizer.Bounds.extents*/ + Offset;
                lineRenderer.SetPosition(1, destinationPosition);
            }

            // Make a loop to the same state with the line renderer if both states are the same
            if (originStateType == destinationStateType) {
                Vector3 firstPoint = lineRenderer.GetPosition(0);
                Vector3 lastPoint = lineRenderer.GetPosition(1);

                lineRenderer.positionCount = 4;
                Vector3[] newPositions = new Vector3[4];
                newPositions[0] = firstPoint;
                newPositions[1] = firstPoint + transform.right * -5 + transform.up * 1;
                newPositions[2] = firstPoint + transform.right * -5 + transform.up * -1;
                newPositions[3] = lastPoint;
                lineRenderer.SetPositions(newPositions);
            }
        }
    }
}