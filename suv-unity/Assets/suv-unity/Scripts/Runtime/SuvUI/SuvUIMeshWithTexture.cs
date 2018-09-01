using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteUV
{
    public class SuvUIMeshWithTexture : SuvUIMeshGraphic
    {
        public override Texture mainTexture
        {
            get
            {
                if (!m_MeshWithTexture || !m_MeshWithTexture.texture)
                {
                    if (material && material.mainTexture)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_MeshWithTexture.texture;
            }
        }

        [Header("Mesh with texture settings")]
        [SerializeField]
        SuvMeshWithTexture m_MeshWithTexture;
        protected override Mesh meshRef
        {
            get
            {
                return m_MeshWithTexture ? m_MeshWithTexture.mesh : null;
            }
        }
        public SuvMeshWithTexture meshWithTexture
        {
            get
            {
                return m_MeshWithTexture;
            }
            set
            {
                if (m_MeshWithTexture == value)
                {
                    return;
                }
                m_MeshWithTexture = value;
                SetVerticesDirty();
            }
        }

    }
}