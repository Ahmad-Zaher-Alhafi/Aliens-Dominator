using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GWindZone))]
    public class GWindZoneInspector : Editor
    {
        private static Vector3[] arrowShape = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(-0.25f, 0, 0.5f),
            new Vector3(-0.25f, 0, -1),
            new Vector3(0.25f, 0, -1),
            new Vector3(0.25f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };

        private GWindZone instance;
        private void OnEnable()
        {
            instance = target as GWindZone;
            if (instance.enabled)
            {
                GAnalytics.Record(GAnalytics.WIND_ZONE, true);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            instance.DirectionX = EditorGUILayout.FloatField("Direction X", instance.DirectionX);
            instance.DirectionZ = EditorGUILayout.FloatField("Direction Z", instance.DirectionZ);
            instance.Speed = EditorGUILayout.FloatField("Speed", instance.Speed);
            instance.Spread = EditorGUILayout.FloatField("Spread", instance.Spread);
            if (EditorGUI.EndChangeCheck())
            {
                instance.SyncTransform();
            }
        }

        private void OnSceneGUI()
        {
            instance.SyncDirection();

            Handles.color = instance.enabled ? Color.cyan : Color.gray;
            CompareFunction zTest = Handles.zTest;
            Handles.zTest = CompareFunction.LessEqual;
            Matrix4x4 handlesMatrix = Handles.matrix;

            float arrowSize = 5;
            Handles.matrix = Matrix4x4.TRS(
                instance.transform.position,
                instance.transform.rotation,
                Vector3.one * arrowSize);
            Handles.DrawPolyLine(arrowShape);

            Handles.matrix = Matrix4x4.TRS(
                instance.transform.position,
                instance.transform.rotation * Quaternion.Euler(0, 0, 90),
                Vector3.one * arrowSize);
            Handles.DrawPolyLine(arrowShape);

            Handles.matrix = handlesMatrix;
            Handles.zTest = zTest;
        }
    }
}
