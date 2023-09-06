using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Animations;

public class UpdateAnimatorController : MonoBehaviour {
    [MenuItem("Custom/UpdateAnimatorController")]
    static void UpdateAnimatorControllerReferences() {
        string creaturesName = "Polygonal Alien Ulifo";
        // Specify the path to your Animator Controller
        string animatorControllerPath = $"Assets/Resources/Polygonal Aliens Pack/{creaturesName}/Animators/{creaturesName}.controller"; // Replace with your actual path

        // Load the Animator Controller asset
        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);

        if (animatorController == null) {
            Debug.LogError("Animator Controller not found at path: " + animatorControllerPath);
            return;
        }

        // Get all AnimationClips in the destination folder
        string destinationFolder = $"Assets/Animations/Creatures/{creaturesName}"; // Replace with your destination folder
        string[] animationClipPaths = AssetDatabase.FindAssets("t:AnimationClip", new[] { destinationFolder });

        List<AnimationClip> newAnimationClips = new List<AnimationClip>();

        foreach (string clipPath in animationClipPaths) {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(clipPath));
            if (clip != null) {
                newAnimationClips.Add(clip);
            }
        }

        // Iterate through the Animator Controller layers and states
        foreach (AnimatorControllerLayer layer in animatorController.layers) {
            foreach (ChildAnimatorState state in layer.stateMachine.states) {
                AnimatorState animatorState = state.state;

                // Check if the state has an animation clip reference
                if (animatorState.motion is AnimationClip oldClip) {
                    // Find the corresponding new animation clip
                    AnimationClip newClip = newAnimationClips.Find(clip => clip.name == oldClip.name);

                    if (newClip != null) {
                        // Update the animation clip reference
                        animatorState.motion = newClip;
                        Debug.Log("Updated state: " + animatorState.name + " to use animation clip: " + newClip.name);
                    }
                }
            }
        }

        EditorUtility.SetDirty(animatorController);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}