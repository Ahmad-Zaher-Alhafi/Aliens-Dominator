using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GTreePrototypeGroupInspectorDrawer
    {
        private GTreePrototypeGroup instance;

        public GTreePrototypeGroupInspectorDrawer(GTreePrototypeGroup group)
        {
            instance = group;
        }

        public static GTreePrototypeGroupInspectorDrawer Create(GTreePrototypeGroup group)
        {
            return new GTreePrototypeGroupInspectorDrawer(group);
        }

        public void DrawGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawPrototypesListGUI();
            DrawAddPrototypeGUI();
            if (EditorGUI.EndChangeCheck())
            {
                SetFoliageDirty();
                EditorUtility.SetDirty(instance);
            }
        }

        private void DrawPrototypesListGUI()
        {
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GTreePrototype p = instance.Prototypes[i];
                CachePrefabPath(p);

                string label = p.Prefab != null && !string.IsNullOrEmpty(p.Prefab.name) ? p.Prefab.name : "Tree " + i;
                string id = "treeprototype" + i + instance.GetInstanceID().ToString();

                int index = i;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Remove"),
                    false,
                    () => { ConfirmAndRemovePrototypeAtIndex(index); });

                GEditorCommon.Foldout(label, false, id, () =>
                {
                    if (p.Prefab != null)
                    {
                        DrawPreview(p.Prefab);
                    }

                    EditorGUI.BeginChangeCheck();
                    p.Prefab = EditorGUILayout.ObjectField("Prefab", p.Prefab, typeof(GameObject), false) as GameObject;
                    p.Billboard = EditorGUILayout.ObjectField("Billboard", p.Billboard, typeof(BillboardAsset), false) as BillboardAsset;
                    p.PivotOffset = EditorGUILayout.Slider("Pivot Offset", p.PivotOffset, -1f, 1f);
                    p.BaseRotation = Quaternion.Euler(GEditorCommon.InlineVector3Field("Base Rotation", p.BaseRotation.eulerAngles));
                    p.BaseScale = GEditorCommon.InlineVector3Field("Base Scale", p.BaseScale);
                    GUI.enabled = !p.KeepPrefabLayer;
                    p.Layer = EditorGUILayout.LayerField("Layer", p.Layer);
                    GUI.enabled = true;
                    p.KeepPrefabLayer = EditorGUILayout.Toggle("Keep Prefab Layer", p.KeepPrefabLayer);
                    GUI.enabled = false;
                    EditorGUILayout.Toggle("Has Collider", p.HasCollider);
                    GUI.enabled = true;
                    if (EditorGUI.EndChangeCheck())
                    {
                        p.Refresh();
                    }
                }, menu);
            }
        }

        private void CachePrefabPath(GTreePrototype p)
        {
            if (p.Prefab == null)
            {
                p.Editor_PrefabAssetPath = null;
            }
            else
            {
                p.Editor_PrefabAssetPath = AssetDatabase.GetAssetPath(p.Prefab);
            }
        }

        private void ConfirmAndRemovePrototypeAtIndex(int index)
        {
            GTreePrototype p = instance.Prototypes[index];
            string label = p.Prefab != null ? p.Prefab.name : "Tree " + index;
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Remove " + label,
                "OK", "Cancel"))
            {
                instance.Prototypes.RemoveAt(index);
            }
        }

        private void DrawPreview(GameObject g)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.selectionGridTileSizeMedium.y));
            GEditorCommon.DrawPreview(r, g);
        }

        private void DrawAddPrototypeGUI()
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GameObject g = GEditorCommon.ObjectSelectorDragDrop<GameObject>(r, "Drop a Game Object here!", "t:GameObject");
            if (g != null)
            {
                GTreePrototype p = GTreePrototype.Create(g);
                instance.Prototypes.Add(p);
            }
        }

        private void SetFoliageDirty()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null)
                {
                    t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                }
            }
        }
    }
}
