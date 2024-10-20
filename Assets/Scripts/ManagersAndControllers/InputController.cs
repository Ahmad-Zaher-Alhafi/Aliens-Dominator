using Context;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagersAndControllers {
    public class InputController : MonoBehaviour {
        /// <summary>
        /// Returns the raycast hit point, if nothing is hit then it returns 0.0.0
        /// </summary>
        /// <param name="layerNumber">If null then the raycast will hit all the layers</param>
        /// <returns></returns>
        public Vector3 GetMouseWorldHitPoint(int? layerNumber = null) {
            Camera localActiveCamera = Ctx.Deps.CameraController.LocalActiveCamera;
            Ray ray = localActiveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (layerNumber != null) {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << layerNumber.Value)) return hit.point;
            } else {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return hit.point;
            }

            return Vector3.zero;
        }
    }
}