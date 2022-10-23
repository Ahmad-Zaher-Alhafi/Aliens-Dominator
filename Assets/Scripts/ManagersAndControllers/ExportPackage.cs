using UnityEditor;
using UnityEngine;

namespace ManagersAndControllers {
    public static class ExportPackage {
        [MenuItem("Export/Export with tags and layers, Input settings")]
        public static void Export(string exportedPackageName) {
            string[] projectContent = { "Assets", "Packages", "ProjectSettings/TagManager.asset", "ProjectSettings/InputManager.asset", "ProjectSettings/ProjectSettings.asset" };
            AssetDatabase.ExportPackage(projectContent, exportedPackageName + ".unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log("Project Exported");
        }
    }
}