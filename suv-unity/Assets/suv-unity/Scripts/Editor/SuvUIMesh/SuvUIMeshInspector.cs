using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace SpriteUV
{
	[CustomEditor(typeof(SuvUIMeshGraphic), true)]
	[CanEditMultipleObjects]
    public class SuvUIMeshInspector : GraphicEditor
    {
		protected override void OnEnable()
        {
            base.OnEnable();
			base.SetShowNativeSize(true, true);
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();
			NativeSizeButtonGUI();
			serializedObject.ApplyModifiedProperties();
		}
    }
}
