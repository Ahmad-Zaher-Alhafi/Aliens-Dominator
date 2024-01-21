#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ForceChildOrder : MonoBehaviour {
    private const string MenuName = "Tools/Force Child Sibling Order/";
    
    [MenuItem(MenuName + "As first child")]
    private static void SetAsFirstChild() {
        GameObject selectedObject = Selection.activeGameObject;

        if (!CanChangeSiblingIndex(selectedObject)) return;

        selectedObject.transform.SetAsFirstSibling();
    }
    
    [MenuItem(MenuName + "As last child")]
    private static void SetAsLastChild() {
        GameObject selectedObject = Selection.activeGameObject;

        if (!CanChangeSiblingIndex(selectedObject)) return;

        selectedObject.transform.SetAsLastSibling();
    }

    [MenuItem(MenuName + "Switch with sibling above")]
    private static void SwitchWithSiblingAbove() {
        GameObject selectedObject = Selection.activeGameObject;

        if (!CanChangeSiblingIndex(selectedObject)) return;

        int siblingIndex = selectedObject.transform.GetSiblingIndex();
        selectedObject.transform.SetSiblingIndex(siblingIndex - 1);
    }

    [MenuItem(MenuName + "Switch with sibling under")]
    private static void SwitchWithSiblingUnder() {
        GameObject selectedObject = Selection.activeGameObject;

        if (!CanChangeSiblingIndex(selectedObject)) return;

        int siblingIndex = selectedObject.transform.GetSiblingIndex();
        selectedObject.transform.SetSiblingIndex(siblingIndex + 1);
    }

    /// <summary>
    /// If the object not selected or has no parent then its sibling index can not be changed
    /// </summary>
    private static bool CanChangeSiblingIndex(GameObject selectedObject) {
        selectedObject = Selection.activeGameObject;

        if (selectedObject == null) {
            Debug.Log("No object selected");
            return false;
        }

        if (selectedObject.transform.parent == null) {
            Debug.Log("Object has no parent");
            return false;
        }

        return true;
    }
}

#endif