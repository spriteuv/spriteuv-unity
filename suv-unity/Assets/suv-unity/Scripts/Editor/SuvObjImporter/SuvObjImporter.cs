using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using TinyJson;
using System.Collections.Generic;
using UnityEditor;

namespace SpriteUV
{
    [ScriptedImporter(1, "suvobj")]
    public class SuvObjImporter : ScriptedImporter
    {
        struct SuvObj
        {
            public struct App
            {
                public string name;
                public string ver;
            }

            public struct Mat
            {
                public string id;
                public string name;
                public string[] texFiles;
            }

            public struct Mesh
            {
                public string id;
                public string name;

                public float[] v2;
                public float[] uv;
                public int[] tri;
                public Vector3 pos;
                public float rot;
                public float[] vColor;
            }

            public App app;
            public Mat mat;
            public Mesh[] mesh;
        }

        public enum MaterialSettings
        {
            None = 0, Import = 1, External = 2, FromRenderer = 3
        }

        [System.Serializable]
        public struct ObjectImportSettings
        {
            public bool m_ImportObject;
            public MaterialSettings m_materialSettings;
            public Material m_materialExternal;
            public Shader m_materialShader;
            [Tooltip("MeshRenderer component settings reference")]
            public MeshRenderer m_meshRendererRef;
        }

        public float m_scale = 1;

        public bool m_ImportMeshWithTexture;
        public Texture2D m_MainTexture;
        public ObjectImportSettings m_objImportSettings;

        private const string ROOT_ID = "root";

        List<Vector3> buffMeshVertList = new List<Vector3>();
        List<Vector2> buffMeshUvList = new List<Vector2>();
        List<int> buffMeshTriList = new List<int>();
        List<Color> buffMeshColorList = new List<Color>();

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string fileJson = File.ReadAllText(ctx.assetPath);
            var suvObj = fileJson.FromJson<SuvObj>();

            var suvObjMeshList = suvObj.mesh;
            var rootObj = new GameObject(Path.GetFileNameWithoutExtension(ctx.assetPath));
            var rootObjTransform = rootObj.transform;

            Material mat = null;

            switch (m_objImportSettings.m_materialSettings)
            {
                case MaterialSettings.Import:
                    var suvMat = suvObj.mat;
                    mat = BuildMaterial(ctx, suvMat);
                    ctx.AddObjectToAsset(suvMat.id, mat);
                    break;
                case MaterialSettings.External:
                    mat = m_objImportSettings.m_materialExternal;
                    break;
            }

            var meshRendererRef = m_objImportSettings.m_meshRendererRef;

            for (int i = 0, l = suvObjMeshList.Length; i < l; i++)
            {
                var suvMesh = suvObjMeshList[i];
                Mesh mesh = BuildMesh(suvMesh);
                ctx.AddObjectToAsset(suvMesh.id + "_msh", mesh);

                if (m_ImportMeshWithTexture)
                {
                    SuvMeshWithTexture meshWithTexture = ScriptableObject.CreateInstance<SuvMeshWithTexture>();
                    meshWithTexture.name = suvMesh.name;
                    meshWithTexture.mesh = mesh;
                    meshWithTexture.texture = m_MainTexture;
                    ctx.AddObjectToAsset(suvMesh.id + "_mwt", meshWithTexture);
                }

                if (m_objImportSettings.m_ImportObject)
                {
                    var go = new GameObject(suvMesh.name);
                    go.hideFlags = HideFlags.NotEditable;
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var goRenderer = go.AddComponent<MeshRenderer>();

                    if (meshRendererRef)
                    {
                        goRenderer.lightProbeUsage = meshRendererRef.lightProbeUsage;
                        goRenderer.probeAnchor = meshRendererRef.probeAnchor;
                        goRenderer.lightProbeProxyVolumeOverride = meshRendererRef.lightProbeProxyVolumeOverride;
                        goRenderer.motionVectorGenerationMode = meshRendererRef.motionVectorGenerationMode;
                        goRenderer.allowOcclusionWhenDynamic = meshRendererRef.allowOcclusionWhenDynamic;
                        goRenderer.additionalVertexStreams = meshRendererRef.additionalVertexStreams;
                        goRenderer.receiveShadows = meshRendererRef.receiveShadows;
                        goRenderer.reflectionProbeUsage = meshRendererRef.reflectionProbeUsage;
                        goRenderer.shadowCastingMode = meshRendererRef.shadowCastingMode;
                        goRenderer.tag = meshRendererRef.tag;
                        goRenderer.renderingLayerMask = meshRendererRef.renderingLayerMask;

                        if (m_objImportSettings.m_materialSettings == MaterialSettings.FromRenderer)
                        {
                            goRenderer.sharedMaterials = meshRendererRef.sharedMaterials;
                        }
                    }

                    if (m_objImportSettings.m_materialSettings != MaterialSettings.FromRenderer)
                    {
                        goRenderer.sharedMaterial = mat;
                    }

                    var goTransform = go.transform;
                    goTransform.SetParent(rootObjTransform);
                    goTransform.SetPositionAndRotation(suvMesh.pos, Quaternion.AngleAxis(suvMesh.rot, new Vector3(0, 0, 1)));
                }
            }
            ctx.AddObjectToAsset(ROOT_ID, rootObj);
            ctx.SetMainObject(rootObj);
        }

        private Material BuildMaterial(AssetImportContext ctx, SuvObj.Mat suvMat)
        {
            Material mat;
            if (!m_objImportSettings.m_materialShader)
            {
                mat = new Material(Shader.Find("Unlit/Transparent"));
            }
            else
            {
                mat = new Material(m_objImportSettings.m_materialShader);
            }
            mat.hideFlags = HideFlags.NotEditable;
            mat.name = suvMat.name;
            if (suvMat.texFiles != null && suvMat.texFiles.Length > 0)
            {
                var suvMatTxPath = suvMat.texFiles[0];
                var texturePath = Path.Combine(Path.GetDirectoryName(ctx.assetPath), suvMatTxPath);
                var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
                mat.mainTexture = mainTexture;
            }

            return mat;
        }

        private Mesh BuildMesh(SuvObj.Mesh suvMesh)
        {
            buffMeshVertList.Clear();
            buffMeshUvList.Clear();
            buffMeshTriList.Clear();
            buffMeshColorList.Clear();

            var suvMeshVertArr = suvMesh.v2;
            for (int n = 0, ln = suvMeshVertArr.Length; n < ln; n += 2)
            {
                buffMeshVertList.Add(new Vector3(suvMeshVertArr[n] * m_scale, suvMeshVertArr[n + 1] * m_scale));
            }
            var suvMeshUvArr = suvMesh.uv;
            for (int n = 0, ln = suvMeshUvArr.Length; n < ln; n += 2)
            {
                buffMeshUvList.Add(new Vector2(suvMeshUvArr[n], suvMeshUvArr[n + 1]));
            }
            var suvMeshTriArr = suvMesh.tri;
            for (int n = 0, ln = suvMeshTriArr.Length; n < ln; n++)
            {
                buffMeshTriList.Add(suvMeshTriArr[n]);
            }
            var suvMeshColorArr = suvMesh.vColor;
            if (suvMeshColorArr != null)
            {
                for (int n = 0, ln = suvMeshColorArr.Length; n < ln; n += 4)
                {
                    var c = new Color();
                    c.r = suvMeshColorArr[n];
                    c.g = suvMeshColorArr[n + 1];
                    c.b = suvMeshColorArr[n + 2];
                    c.a = suvMeshColorArr[n + 3];
                    buffMeshColorList.Add(c);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(buffMeshVertList);
            mesh.SetUVs(0, buffMeshUvList);
            mesh.SetTriangles(buffMeshTriList, 0);
            mesh.SetColors(buffMeshColorList);

            mesh.name = suvMesh.name;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.hideFlags = HideFlags.NotEditable;
            return mesh;
        }
    }
}
