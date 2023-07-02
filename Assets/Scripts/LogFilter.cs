#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LogFilter : MonoBehaviour {
    private GameObject lastSelectedObject;

    private void Start() {
        Application.logMessageReceived += HandleLogMessage;
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged() {
        if (Selection.activeGameObject == null) return;
        lastSelectedObject = Selection.activeGameObject;
    }

    private void HandleLogMessage(string logMessage, string stackTrace, LogType logType) {
        if (lastSelectedObject == null) return;
        if (logMessage.Contains(lastSelectedObject.GetInstanceID().ToString())) {
            Debug.LogWarning($"<color=green>{logMessage}</color>", lastSelectedObject);
        }
    }

    private void OnDestroy() {
        Application.logMessageReceived -= HandleLogMessage;
        Selection.selectionChanged -= OnSelectionChanged;
    }
}
#endif