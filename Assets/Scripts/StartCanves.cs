using ManagersAndControllers;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StartCanves : MonoBehaviour {

    public void ExportProject(string exportedPackageName) {
        ExportPackage.Export(exportedPackageName);
    }

    public void StartGameScene() {
        SceneManager.LoadScene("Game Scene");
    }
}