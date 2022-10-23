using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    [CustomEditor(typeof(JDayNightCycle))]
    public class JDayNightCycleInspector : Editor
    {
        private JDayNightCycle instance;
        bool isTimeFoldoutExpanded;

        private void OnEnable()
        {
            instance = target as JDayNightCycle;
        }

        public override void OnInspectorGUI()
        {
            instance.Profile = EditorGUILayout.ObjectField("Profile", instance.Profile, typeof(JDayNightCycleProfile), false) as JDayNightCycleProfile;
            if (instance.Profile == null)
                return;

            DrawSceneReferencesGUI();
            DrawTimeGUI();
            JDayNightCycleProfileInspectorDrawer.Create(instance.Profile).DrawGUI();
        }

        private void DrawSceneReferencesGUI()
        {
            string label = "Scene References";
            string id = "scene-ref" + instance.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.Sky = EditorGUILayout.ObjectField("Sky", instance.Sky, typeof(JSky), true) as JSky;
                instance.OrbitPivot = EditorGUILayout.ObjectField("Orbit Pivot", instance.OrbitPivot, typeof(Transform), true) as Transform;
            });
        }

        private void DrawTimeGUI()
        {
            string label = "Time";
            string id = "time" + instance.GetInstanceID();

            isTimeFoldoutExpanded = JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.StartTime = EditorGUILayout.FloatField("Start Time", instance.StartTime);
                instance.TimeIncrement = EditorGUILayout.FloatField("Time Increment", instance.TimeIncrement);
                GUI.enabled = !instance.AutoTimeIncrement;
                instance.Time = EditorGUILayout.Slider("Time", instance.Time, 0f, 24f);
                GUI.enabled = true;
                instance.AutoTimeIncrement = EditorGUILayout.Toggle("Auto", instance.AutoTimeIncrement);
            });
        }

        public override bool RequiresConstantRepaint()
        {
            return isTimeFoldoutExpanded;
        }

        private void OnSceneGUI()
        {
            Vector3 normal = instance.OrbitPivot ? instance.OrbitPivot.right : Vector3.right;
            Handles.color = Color.green;
            float radius = 10;
            Handles.DrawWireDisc(instance.transform.position, normal, radius);

            float evalTime = Mathf.InverseLerp(0f, 24f, instance.Time);

            if (instance.Sky.Profile.EnableSun && instance.Sky.SunLightSource != null)
            {
                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.up);

                Matrix4x4 localToWorld = instance.OrbitPivot ?
                    instance.OrbitPivot.localToWorldMatrix :
                    Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);

                Vector3 worldPos = instance.transform.position - worldDirection * radius;
                Color c = instance.Sky.Profile.SunColor;
                c.a = Mathf.Max(0.1f, c.a);
                Handles.color = c;
                Handles.DrawSolidDisc(worldPos, normal, 1);
            }

            if (instance.Sky.Profile.EnableMoon && instance.Sky.MoonLightSource != null)
            {
                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.down);

                Matrix4x4 localToWorld = instance.OrbitPivot ?
                    instance.OrbitPivot.localToWorldMatrix :
                    Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);

                Vector3 worldPos = instance.transform.position - worldDirection * radius;
                Color c = instance.Sky.Profile.MoonColor;
                c.a = Mathf.Max(0.1f, c.a);
                Handles.color = c;
                Handles.DrawSolidDisc(worldPos, normal, 1);
            }
        }
    }
}
