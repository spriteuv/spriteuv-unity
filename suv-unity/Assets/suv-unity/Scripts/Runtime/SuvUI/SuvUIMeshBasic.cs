using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteUV
{
    public class SuvUIMeshBasic : SuvUIMeshGraphic
    {
        public override Texture mainTexture
        {
            get
            {
                if (!m_Texture)
                {
                    if (material && material.mainTexture)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Texture;
            }
        }

        [Header("Mesh settings")]
        [SerializeField]
        Texture m_Texture;
        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                {
                    return;
                }

                m_Texture = value;
            }
        }

        [SerializeField]
        Mesh m_Mesh;
        protected override Mesh meshRef
        {
            get
            {
                return m_Mesh;
            }
        }
        public Mesh mesh
        {
            get
            {
                return m_Mesh;
            }
            set
            {
                if (m_Mesh == value)
                {
                    return;
                }
                m_Mesh = value;
                SetVerticesDirty();
            }
        }

    }
}