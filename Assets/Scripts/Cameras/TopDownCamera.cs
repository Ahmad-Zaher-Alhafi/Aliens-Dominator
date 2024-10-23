using Context;
using ManagersAndControllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cameras {
    public class TopDownCamera : MonoBehaviour {
        [Tooltip("How much you should more your mouse to start dragging the camera")]
        [SerializeField] private float dragSensitivity = .1f;

        [Tooltip("X,Z sets how much left and down can the camera go from its initial position, Y forms the zoom level limitation")]
        [SerializeField] private Vector3 topDownCameraMinBoundaries = Vector3.one * 10;
        [Tooltip("X,Z sets how much right and up can the camera go from its initial position, Y forms the zoom level limitation")]
        [SerializeField] private Vector3 topDownCameraMaxBoundaries = Vector3.one * 10;
        [SerializeField] private float topDownCameraDragSpeed = 3;
        [SerializeField] private float topDownCameraZoomSpeed = 3;

        private float CurrentZoomLevel => initialTopDownCameraPosition.y - transform.position.y;
        private Vector3 TopDownCameraMinPosition => initialTopDownCameraPosition - topDownCameraMinBoundaries - new Vector3(CurrentZoomLevel, 0, CurrentZoomLevel);
        private Vector3 TopDownCameraMaxPosition => initialTopDownCameraPosition + topDownCameraMaxBoundaries + new Vector3(CurrentZoomLevel, 0, CurrentZoomLevel);

        private Vector3 initialTopDownCameraPosition;
        private Vector3 topDownCameraTargetPosition;

        private void Awake() {
            initialTopDownCameraPosition = transform.position;
            topDownCameraTargetPosition = initialTopDownCameraPosition;

            Ctx.Deps.InputActions.TopDownViewActions.DragCamera.performed += OnCameraDrag;
            Ctx.Deps.InputActions.TopDownViewActions.ZoomCamera.performed += OnCameraZoom;
        }

        private void OnCameraDrag(InputAction.CallbackContext obj) {
            Vector2 dragDelta = Ctx.Deps.InputActions.TopDownViewActions.DragCamera.ReadValue<Vector2>();
            if (!HasMovedMouseEnough(dragDelta)) return;
            topDownCameraTargetPosition += -(transform.right * dragDelta.x + transform.up * dragDelta.y) * topDownCameraDragSpeed;
        }

        private void OnCameraZoom(InputAction.CallbackContext obj) {
            Vector2 zoomDelta = Ctx.Deps.InputActions.TopDownViewActions.ZoomCamera.ReadValue<Vector2>();
            topDownCameraTargetPosition += (-transform.forward * zoomDelta.x + transform.forward * zoomDelta.y) * topDownCameraZoomSpeed;
        }

        private void Update() {
            if (Ctx.Deps.GameController.CurrentViewMode is not GameController.ViewMode.TopDown) return;

            InterpolateCameraPositionAndZoom();
        }

        private void InterpolateCameraPositionAndZoom() {
            topDownCameraTargetPosition.x = Mathf.Clamp(topDownCameraTargetPosition.x, TopDownCameraMinPosition.x, TopDownCameraMaxPosition.x);
            topDownCameraTargetPosition.y = Mathf.Clamp(topDownCameraTargetPosition.y, TopDownCameraMinPosition.y, TopDownCameraMaxPosition.y);
            topDownCameraTargetPosition.z = Mathf.Clamp(topDownCameraTargetPosition.z, TopDownCameraMinPosition.z, TopDownCameraMaxPosition.z);

            transform.position = Vector3.LerpUnclamped(transform.position, topDownCameraTargetPosition, .1f);
        }

        private bool HasMovedMouseEnough(Vector2 dragDelta) {
            return Mathf.Abs(dragDelta.x) >= dragSensitivity || Mathf.Abs(dragDelta.y) >= dragSensitivity;
        }

        private void OnDestroy() {
            Ctx.Deps.InputActions.TopDownViewActions.DragCamera.performed -= OnCameraDrag;
            Ctx.Deps.InputActions.TopDownViewActions.ZoomCamera.performed -= OnCameraZoom;
        }
    }
}