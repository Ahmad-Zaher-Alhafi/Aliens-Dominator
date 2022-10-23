using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    [CustomEditor(typeof(JSky))]
    public class JSkyInspector : Editor
    {
        private JSky instance;

        private void OnEnable()
        {
            instance = target as JSky;
        }

        public override void OnInspectorGUI()
        {
            instance.Profile = EditorGUILayout.ObjectField("Profile", instance.Profile, typeof(JSkyProfile), false) as JSkyProfile;
            if (instance.Profile == null)
                return;

            DrawSceneReferencesGUI();
            JSkyProfileInspectorDrawer.Create(instance.Profile).DrawGUI();
        }

        private void DrawSceneReferencesGUI()
        {
            string label = "Scene References";
            string id = "scene-references";

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.SunLightSource = EditorGUILayout.ObjectField("Sun Light Source", instance.SunLightSource, typeof(Light), true) as Light;
                instance.MoonLightSource = EditorGUILayout.ObjectField("Moon Light Source", instance.MoonLightSource, typeof(Light), true) as Light;
            });
        }
    }
}
