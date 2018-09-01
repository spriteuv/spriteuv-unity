using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteUV
{
    [CustomEditor(typeof(SuvObjImporter))]
    public class SuvObjImporterEditor : ScriptedImporterEditor
    {
        SerializedProperty pScale;

        SerializedProperty pObjectImportSettings;
        SerializedProperty pImportMeshWithTexture;
        SerializedProperty pMainTexture;

        SerializedProperty pMaterialSettings;
        SerializedProperty pMaterialShader;
        SerializedProperty pMaterialExternal;
        SerializedProperty pImportObject;
        SerializedProperty pMeshRendererRef;

        public override void OnEnable()
        {
            base.OnEnable();
            pScale = serializedObject.FindProperty("m_scale");

            pImportMeshWithTexture = serializedObject.FindProperty("m_ImportMeshWithTexture");
            pMainTexture = serializedObject.FindProperty("m_MainTexture");

            pObjectImportSettings = serializedObject.FindProperty("m_objImportSettings");

            pImportObject = pObjectImportSettings.FindPropertyRelative("m_ImportObject");
            pMeshRendererRef = pObjectImportSettings.FindPropertyRelative("m_meshRendererRef");

            pMaterialSettings = pObjectImportSettings.FindPropertyRelative("m_materialSettings");
            pMaterialShader = pObjectImportSettings.FindPropertyRelative("m_materialShader");
            pMaterialExternal = pObjectImportSettings.FindPropertyRelative("m_materialExternal");
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(pScale);

            EditorGUILayout.PropertyField(pImportMeshWithTexture);
            if (pImportMeshWithTexture.boolValue)
            {
                EditorGUILayout.PropertyField(pMainTexture);
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(pImportObject);
            if (pImportObject.boolValue)
            {
                EditorGUILayout.PropertyField(pMeshRendererRef);
            }

            EditorGUILayout.Separator();

            var nextMaterialSettings = (SuvObjImporter.MaterialSettings)EditorGUILayout.EnumPopup(pMaterialSettings.displayName, (SuvObjImporter.MaterialSettings)pMaterialSettings.intValue);
            pMaterialSettings.intValue = (int)nextMaterialSettings;
            switch (nextMaterialSettings)
            {
                case SuvObjImporter.MaterialSettings.External:
                    EditorGUILayout.PropertyField(pMaterialExternal);
                    break;
                case SuvObjImporter.MaterialSettings.Import:
                    EditorGUILayout.PropertyField(pMaterialShader);
                    break;
            }

            base.ApplyRevertGUI();
        }
    }
}