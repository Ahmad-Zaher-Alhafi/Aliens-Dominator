using Context;
using ManagersAndControllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cameras {
    public class TopDownCamera : MonoBehaviour {
        [SerializeField] private Vector3 topDownCameraMinOffset = Vector3.one * 10;
        [SerializeField] private Vector3 topDownCameraMaxOffset = Vector3.one * 10;
        [SerializeField] private float topDownCameraDragSpeed = 3;
        [SerializeField] private float topDownCameraZoomSpeed = 3;

        private float CurrentZoomLevel => initialTopDownCameraPosition.y - transform.position.y;
        private Vector3 TopDownCameraMinPosition => initialTopDownCameraPosition - topDownCameraMinOffset - new Vector3(CurrentZoomLevel, 0, CurrentZoomLevel);
        private Vector3 TopDownCameraMaxPosition => initialTopDownCameraPosition + topDownCameraMaxOffset + new Vector3(CurrentZoomLevel, 0, CurrentZoomLevel);

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

        private void OnDestroy() {
            Ctx.Deps.InputActions.TopDownViewActions.DragCamera.performed -= OnCameraDrag;
            Ctx.Deps.InputActions.TopDownViewActions.ZoomCamera.performed -= OnCameraZoom;
        }
    }
}