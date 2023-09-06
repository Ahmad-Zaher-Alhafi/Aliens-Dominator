using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimationClipCopy : MonoBehaviour {
    [MenuItem("Custom/CopyAnimationClips")]
    static void CopyAnimationClips() {
        // Specify the root folder to search for folders containing FBX asset bundles
        string rootFolder = "Assets/Resources/Polygonal Aliens Pack/Polygonal Alien Magantis"; // Replace with your actual root folder path

        // Specify the destination folder
        string destinationRootFolder = "Assets/Animations/Creatures";


        // Scan for folders containing FBX files recursively in the root folder
        string[] sourceFolders = Directory.GetDirectories(rootFolder);

        foreach (string sourceFolder in sourceFolders) {
            // Get the folder name (assuming it's the name you want to use for the destination folder)
            string folderName = Path.GetFileName(sourceFolder);

            // Create a destination folder with the same name
            string destinationFolder = Path.Combine(destinationRootFolder, folderName);

            // Create the destination folder if it doesn't exist
            if (!Directory.Exists(destinationFolder)) {
                Directory.CreateDirectory(destinationFolder);
            }

            // Create a dictionary to store FBX asset bundle information
            Dictionary<string, string> bundlePaths = new Dictionary<string, string>();

            // Scan for FBX files in the current source folder
            string[] fbxFiles = Directory.GetFiles(sourceFolder, "*.fbx", SearchOption.AllDirectories);

            foreach (string fbxPath in fbxFiles) {
                // Extract bundle name from the path (you can implement your naming convention here)
                string bundleName = Path.GetFileNameWithoutExtension(fbxPath);

                // Store the bundle name and path in the dictionary
                bundlePaths[bundleName] = fbxPath;
            }

            foreach (var kvp in bundlePaths) {
                string bundleName = kvp.Key;
                string fbxPath = kvp.Value;

                // Load the FBX file as a GameObject
                var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxPath);

                foreach (var assetRepresentation in assetRepresentationsAtPath) {
                    var animationClip = assetRepresentation as AnimationClip;

                    if (animationClip == null) continue;

                    // Create a copy of the animation clip
                    AnimationClip copiedClip = Instantiate(animationClip);

                    // Generate a unique name for the copied clip based on the bundle name and animation name
                    string clipName = bundleName + "_" + animationClip.name + ".anim";

                    // Save the copied clip to the destination folder
                    string destinationPath = Path.Combine(destinationFolder, clipName);
                    AssetDatabase.CreateAsset(copiedClip, destinationPath);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}