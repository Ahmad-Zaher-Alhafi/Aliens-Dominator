#if UNITY_EDITOR
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GLivePreviewDrawer
    {
        private static MaterialPropertyBlock previewPropertyBlock = new MaterialPropertyBlock();

        public static void DrawGeometryLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            Mesh previewMesh = GGriffinSettings.Instance.GetLivePreviewMesh(t.TerrainData.Geometry.MeshResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetTexture("_OldHeightMap", t.TerrainData.Geometry.HeightMap);
            previewPropertyBlock.SetTexture("_NewHeightMap", newHeightMap);
            previewPropertyBlock.SetTexture("_MainTex", newHeightMap);
            previewPropertyBlock.SetFloat("_Height", t.TerrainData.Geometry.Height);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            Material mat = GInternalMaterials.GeometryLivePreviewMaterial;
            mat.renderQueue = 4000;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    Graphics.DrawMesh(
                        previewMesh,
                        Matrix4x4.TRS(worldPos, rotation, scale),
                        mat,
                        LayerMask.NameToLayer("Default"),
                        cam,
                        0,
                        previewPropertyBlock);
                }
            }
        }

        public static void DrawSubdivLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);

            GPolygonDistributionMode polygonMode = t.TerrainData.Geometry.PolygonDistribution;
            int baseResolution = polygonMode == GPolygonDistributionMode.Dynamic ?
                t.TerrainData.Geometry.MeshBaseResolution :
                t.TerrainData.Geometry.MeshResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);
            previewPropertyBlock.SetTexture("_Mask", mask);
            previewPropertyBlock.SetMatrix("_WorldPointToMask", worldPointToMaskMatrix);

            Material mat = GInternalMaterials.SubdivLivePreviewMaterial;
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    for (int i = baseResolution; i <= maxLevel; ++i)
                    {
                        previewMesh = GGriffinSettings.Instance.GetLivePreviewWireframeMesh(i);
                        previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                            (i - baseResolution) * GCommon.SUB_DIV_STEP,
                            i != maxLevel ? (i - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                            0, 0));

                        Graphics.DrawMesh(
                            previewMesh,
                            Matrix4x4.TRS(worldPos, rotation, scale),
                            mat,
                            LayerMask.NameToLayer("Default"),
                            cam,
                            0,
                            previewPropertyBlock);
                    }
                }
            }
        }

        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);
            GPolygonDistributionMode polygonMode = t.TerrainData.Geometry.PolygonDistribution;
            int baseResolution = polygonMode == GPolygonDistributionMode.Dynamic ?
                t.TerrainData.Geometry.MeshBaseResolution :
                t.TerrainData.Geometry.MeshResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);
            previewPropertyBlock.SetTexture("_Mask", mask);
            previewPropertyBlock.SetMatrix("_WorldPointToMask", worldPointToMaskMatrix);

            Material mat = GInternalMaterials.VisibilityLivePreviewMaterial;
            mat.EnableKeyword("USE_MASK");
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    for (int i = baseResolution; i <= maxLevel; ++i)
                    {
                        previewMesh = GGriffinSettings.Instance.GetLivePreviewWireframeMesh(i);
                        previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                            (i - baseResolution) * GCommon.SUB_DIV_STEP,
                            i != maxLevel ? (i - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                            0, 0));

                        Graphics.DrawMesh(
                            previewMesh,
                            Matrix4x4.TRS(worldPos, rotation, scale),
                            mat,
                            LayerMask.NameToLayer("Default"),
                            cam,
                            0,
                            previewPropertyBlock);
                    }
                }
            }
        }

        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            t.TerrainData.Geometry.Internal_CreateNewSubDivisionMap(newHeightMap);
            GPolygonDistributionMode polygonMode = t.TerrainData.Geometry.PolygonDistribution;
            int baseResolution = polygonMode == GPolygonDistributionMode.Dynamic ?
                t.TerrainData.Geometry.MeshBaseResolution :
                t.TerrainData.Geometry.MeshResolution;
            int resolution = t.TerrainData.Geometry.MeshResolution;
            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GCommon.SUB_DIV_STEP), resolution - baseResolution);

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);
            previewPropertyBlock.Clear();
            previewPropertyBlock.SetColor("_Color", new Color(0, 0, 0, 0.6f));
            previewPropertyBlock.SetTexture("_HeightMap", newHeightMap);
            previewPropertyBlock.SetVector("_Dimension", terrainSize);
            previewPropertyBlock.SetVector("_BoundMin", t.transform.position);
            previewPropertyBlock.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            previewPropertyBlock.SetTexture("_SubdivMap", t.TerrainData.Geometry.Internal_SubDivisionMap);

            Material mat = GInternalMaterials.VisibilityLivePreviewMaterial;
            mat.DisableKeyword("USE_MASK");
            mat.renderQueue = 4000;
            Mesh previewMesh = null;

            int gridSize = t.TerrainData.Geometry.ChunkGridSize;
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    Rect uvRect = GCommon.GetUvRange(gridSize, x, z);
                    if (!uvRect.Overlaps(dirtyRect))
                        continue;
                    Vector3 localPos = new Vector3(
                        terrainSize.x * uvRect.x,
                        0f,
                        terrainSize.z * uvRect.y);
                    Vector3 worldPos = t.transform.TransformPoint(localPos);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = new Vector3(terrainSize.x * uvRect.width, 1, terrainSize.z * uvRect.height);

                    for (int i = baseResolution; i <= maxLevel; ++i)
                    {
                        previewMesh = GGriffinSettings.Instance.GetLivePreviewWireframeMesh(i);
                        previewPropertyBlock.SetVector("_SubdivRange", new Vector4(
                            (i - baseResolution) * GCommon.SUB_DIV_STEP,
                            i != maxLevel ? (i - baseResolution + 1) * GCommon.SUB_DIV_STEP : 1f,
                            0, 0));

                        Graphics.DrawMesh(
                            previewMesh,
                            Matrix4x4.TRS(worldPos, rotation, scale),
                            mat,
                            LayerMask.NameToLayer("Default"),
                            cam,
                            0,
                            previewPropertyBlock);
                    }
                }
            }
        }

        public static void DrawAlbedoLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.AlbedoMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.AlbedoMapPropertyName, newAlbedo);
            }

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    string chunkMeshName = GTerrainChunk.GetChunkMeshName(chunks[i].Index, 0);
                    Mesh chunkMesh = t.TerrainData.GeometryData.GetMesh(chunkMeshName);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawMetallicSmoothnessLivePreview(GStylizedTerrain t, Camera cam, Texture newMetallicMap, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.MetallicMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.MetallicMapPropertyName, newMetallicMap);
            }

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    string chunkMeshName = GTerrainChunk.GetChunkMeshName(chunks[i].Index, 0);
                    Mesh chunkMesh = t.TerrainData.GeometryData.GetMesh(chunkMeshName);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawAMSLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.AlbedoMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.AlbedoMapPropertyName, newAlbedo);
            }
            if (!string.IsNullOrEmpty(t.TerrainData.Shading.MetallicMapPropertyName))
            {
                previewPropertyBlock.SetTexture(t.TerrainData.Shading.MetallicMapPropertyName, newMetallicMap);
            }


            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    string chunkMeshName = GTerrainChunk.GetChunkMeshName(chunks[i].Index, 0);
                    Mesh chunkMesh = t.TerrainData.GeometryData.GetMesh(chunkMeshName);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawSplatLivePreview(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, Rect dirtyRect)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;
            int controlMapResolution = t.TerrainData.Shading.SplatControlResolution;
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            if (controlMapCount == 0)
                return;

            previewPropertyBlock.Clear();
            for (int i = 0; i < controlMapCount; ++i)
            {
                if (!string.IsNullOrEmpty(t.TerrainData.Shading.SplatControlMapPropertyName))
                {
                    previewPropertyBlock.SetTexture(t.TerrainData.Shading.SplatControlMapPropertyName + i, newControlMaps[i]);
                }
            }

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    string chunkMeshName = GTerrainChunk.GetChunkMeshName(chunks[i].Index, 0);
                    Mesh chunkMesh = t.TerrainData.GeometryData.GetMesh(chunkMeshName);
                    if (chunkMesh != null)
                    {
                        Graphics.DrawMesh(
                            chunkMesh,
                            chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f),
                            mat,
                            chunks[i].gameObject.layer,
                            cam,
                            0,
                            previewPropertyBlock,
                            t.TerrainData.Rendering.CastShadow,
                            t.TerrainData.Rendering.ReceiveShadow);
                    }
                }
            }
        }

        public static void DrawMasksLivePreview(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, Rect dirtyRect)
        {
            Material mat = GInternalMaterials.MaskVisualizerMaterial;
            if (mat == null)
                return;

            previewPropertyBlock.Clear();
            previewPropertyBlock.SetVector("_Dimension", new Vector4(t.TerrainData.Geometry.Width, t.TerrainData.Geometry.Height, t.TerrainData.Geometry.Length));

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRange = chunks[i].GetUvRange();
                if (uvRange.Overlaps(dirtyRect))
                {
                    string chunkMeshName = GTerrainChunk.GetChunkMeshName(chunks[i].Index, 0);
                    Mesh chunkMesh = t.TerrainData.GeometryData.GetMesh(chunkMeshName);
                    if (chunkMesh != null)
                    {
                        for (int j = 0; j < masks.Length; ++j)
                        {
                            previewPropertyBlock.SetColor("_Color", colors[j]);
                            previewPropertyBlock.SetTexture("_MainTex", masks[j]);
                            Graphics.DrawMesh(
                                chunkMesh,
                                chunks[i].transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.up * 0.01f * j),
                                mat,
                                chunks[i].gameObject.layer,
                                cam,
                                0,
                                previewPropertyBlock,
                                t.TerrainData.Rendering.CastShadow,
                                t.TerrainData.Rendering.ReceiveShadow);
                        }
                    }
                }
            }
        }
    }
}
#endif
