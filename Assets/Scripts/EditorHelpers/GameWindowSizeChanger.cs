#if UNITY_EDITOR
using UnityEngine;

namespace EditorHelpers {
    public class GameWindowSizeChanger : MonoBehaviour {
        private void Update() {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space)) {
                UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
            }
        }
    }
}
#endif