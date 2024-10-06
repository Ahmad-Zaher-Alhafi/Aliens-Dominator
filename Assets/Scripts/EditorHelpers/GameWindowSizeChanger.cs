#if UNITY_EDITOR
using Context;
using UnityEngine;

namespace EditorHelpers {
    public class GameWindowSizeChanger : MonoBehaviour {
        private void Update() {
            if (Ctx.Deps.InputActions.EditorActions.ChangeGameWindowSize.WasPressedThisFrame()) {
                UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
            }
        }
    }
}
#endif