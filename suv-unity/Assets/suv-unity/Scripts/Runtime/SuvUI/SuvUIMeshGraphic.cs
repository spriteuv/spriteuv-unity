using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteUV
{
    public abstract class SuvUIMeshGraphic : Graphic
    {

        [Header("Mesh Graphic Settings ")]
        [SerializeField]
        bool m_UseRectDimension = true;
        public bool useRectDimension
        {
            get
            {
                return m_UseRectDimension;
            }
            set
            {
                if (m_UseRectDimension == value)
                {
                    return;
                }
                m_UseRectDimension = value;
                SetVerticesDirty();
            }
        }

        protected List<Vector3> meshVertexBuff = new List<Vector3>();
        protected List<int> meshTriBuff = new List<int>();
        protected List<UIVertex> uiVertexBuff = new List<UIVertex>();

        int assignedMeshId = -1;

        protected abstract Mesh meshRef { get; }

        protected SuvUIMeshGraphic()
        {
            useLegacyMeshGeneration = false;
        }

        public override void SetNativeSize()
        {
            if (meshRef)
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = meshRef.bounds.size;
            }
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            if (meshRef && (uiVertexBuff.Count == 0 || assignedMeshId != meshRef.GetInstanceID()))
            {
                RebuildUIVertexBuffer();
            }
            else if (!meshRef)
            {
                uiVertexBuff.Clear();
                meshTriBuff.Clear();
                return;
            }
            UpdateUIVertexBuffer();
        }

        void RebuildUIVertexBuffer()
        {
            uiVertexBuff.Clear();

            this.meshVertexBuff.Clear();
            meshRef.GetVertices(this.meshVertexBuff);

            // if meshRef getting changed often, consider to use List<Vector2> buffer
            var meshUVBuff = meshRef.uv;

            this.meshTriBuff.Clear();
            meshRef.GetTriangles(this.meshTriBuff, 0);

            for (int i = 0, l = meshVertexBuff.Count; i < l; i++)
            {
                var v = meshVertexBuff[i];
                var uiVrt = new UIVertex();
                uiVrt.position = meshVertexBuff[i];
                uiVrt.uv0 = meshUVBuff[i];
                uiVertexBuff.Add(uiVrt);
            }
            assignedMeshId = meshRef.GetInstanceID();
        }


        void UpdateUIVertexBuffer()
        {
            var bounds = meshRef.bounds;
            var mshSize = bounds.size;
            var mshMin = bounds.min;
            var uiRectSize = rectTransform.rect.size;
            float wScale = uiRectSize.x / mshSize.x;
            float hScale = uiRectSize.y / mshSize.y;
            var rectPivot = rectTransform.pivot;
            var mshPivotOffset = new Vector2(rectPivot.x * uiRectSize.x, rectPivot.y * uiRectSize.y);

            if (!useRectDimension)
            {
                mshMin = new Vector2();
                wScale = 1;
                hScale = 1;
                mshPivotOffset = new Vector2();
            }

            for (int i = 0, l = uiVertexBuff.Count; i < l; i++)
            {
                var v = meshVertexBuff[i];
                var uiVertex = uiVertexBuff[i];
                uiVertex.position = new Vector3((v.x - mshMin.x) * wScale - mshPivotOffset.x, (v.y - mshMin.y) * hScale - mshPivotOffset.y, v.z);
                uiVertex.color = color;
                uiVertexBuff[i] = uiVertex;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            vh.AddUIVertexStream(uiVertexBuff, meshTriBuff);
        }
    }
}