#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnitySimplified.VariableReferences;

namespace UnitySimplifiedEditor.VariableReferences
{
    [CustomEditor(typeof(VariableAsset<>), true)]
    public class VariableObjectEditor : Editor
    {
        private SerializedProperty _scriptProp;
        private SerializedProperty _editorDescriptionProp;
        private SerializedProperty _valueProp;
        private SerializedProperty _runtimeValueProp;

        private void OnEnable()
        {
            _scriptProp = serializedObject.FindProperty("m_Script");
            _editorDescriptionProp = serializedObject.FindProperty("editorDescription");
            _valueProp = serializedObject.FindProperty("value");
            _runtimeValueProp = serializedObject.FindProperty("runtimeValue");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_scriptProp);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_editorDescriptionProp);
            if (!EditorApplication.isPlaying)
                EditorGUILayout.PropertyField(_valueProp);
            else EditorGUILayout.PropertyField(_runtimeValueProp, new GUIContent(_valueProp.displayName, _valueProp.tooltip));
            if (!EditorGUI.EndChangeCheck())
                return;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }

}

#endif