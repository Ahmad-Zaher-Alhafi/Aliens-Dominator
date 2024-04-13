using UnityEngine;

namespace EditorHelpers {
    public class GameWindowSizeChanger : MonoBehaviour {
#if UNITY_EDITOR
        private void Update() {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space)) {
                UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
            }
#endif
        }
    }
}