using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    public class GVisibilityPainter : IGTexturePainter, IGTexturePainterWithLivePreview
    {
        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Mark a particular area of terrain surface as visible or not.\n" +
                    "   - Hold Left Mouse to paint.\n" +
                    "   - Hold Ctrl & Left Mouse to erase.");
                return s;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Visibility Painting";
            }
        }

        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args)
        {
            return GCommon.HeightMapAndFoliageResourceFlags;
        }

        public void Paint(GStylizedTerrain terrain, GTexturePainterArgs args)
        {
            if (terrain.TerrainData == null)
                return;
            if (args.MouseEventType == GPainterMouseEventType.Down)
            {
                terrain.ForceLOD(0);
                GGriffinSettings.Instance.IsHidingFoliageOnEditing = true;
            }
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                terrain.ForceLOD(-1);
                GGriffinSettings.Instance.IsHidingFoliageOnEditing = false;
                return;
            }

            Vector2[] uvCorners = new Vector2[args.WorldPointCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = terrain.WorldPointToUV(args.WorldPointCorners[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            Texture2D bg = terrain.TerrainData.Geometry.HeightMap;
            int heightMapResolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(heightMapResolution);
            GCommon.CopyToRT(bg, rt);

            Material mat = GInternalMaterials.VisibilityPainterMaterial;
            mat.SetTexture("_MainTex", bg);
            mat.SetTexture("_Mask", args.Mask);
            mat.SetFloat("_Opacity", args.Opacity);
            int pass =
                args.ActionType == GPainterActionType.Normal ? 0 :
                args.ActionType == GPainterActionType.Negative ? 1 : 0;
            GCommon.DrawQuad(rt, uvCorners, mat, pass);

            RenderTexture.active = rt;
            terrain.TerrainData.Geometry.HeightMap.ReadPixels(
                new Rect(0, 0, heightMapResolution, heightMapResolution), 0, 0);
            terrain.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;

            terrain.TerrainData.Geometry.SetRegionDirty(dirtyRect);
            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
        }

        public void Editor_DrawLivePreview(GStylizedTerrain terrain, GTexturePainterArgs args, Camera cam)
        {
#if UNITY_EDITOR
            Vector2[] uvCorners = new Vector2[args.WorldPointCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = terrain.WorldPointToUV(args.WorldPointCorners[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            Texture2D bg = terrain.TerrainData.Geometry.HeightMap;
            int heightMapResolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, heightMapResolution);
            GCommon.CopyToRT(bg, rt);

            Material mat = GInternalMaterials.VisibilityPainterMaterial;
            mat.SetTexture("_MainTex", bg);
            mat.SetTexture("_Mask", args.Mask);
            mat.SetFloat("_Opacity", args.Opacity);
            int pass =
                args.ActionType == GPainterActionType.Normal ? 0 :
                args.ActionType == GPainterActionType.Negative ? 1 : 0;
            GCommon.DrawQuad(rt, uvCorners, mat, pass);

            Matrix4x4 worldToMaskMatrix = Matrix4x4.TRS(
                args.WorldPointCorners[0],
                Quaternion.Euler(0, args.Rotation, 0),
                args.Radius * 2 * Vector3.one).inverse;

            GLivePreviewDrawer.DrawVisibilityLivePreview(terrain, cam, rt, dirtyRect, args.Mask, worldToMaskMatrix);
#endif
        }
    }
}
