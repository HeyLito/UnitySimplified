#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnitySimplified.VariableReferences;

namespace UnitySimplifiedEditor.VariableReferences
{
    [CustomPropertyDrawer(typeof(VariableReference<,>), true)]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        private readonly string[] _popupOptions = { "Use Constant", "Use Variable" };
        private GUIStyle _popupStyle;

        private GUIStyle PopupStyle => _popupStyle ??= new GUIStyle(GUI.skin.GetStyle("PaneOptions")) 
        {
            imagePosition = ImagePosition.ImageOnly
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUIUtility.singleLineHeight;
            SerializedProperty useConstantProp = property.FindPropertyRelative("_useConstant");
            if (useConstantProp.boolValue)
            {
                SerializedProperty constantValueProp = property.FindPropertyRelative("_constantValue");
                float constantValueHeight = EditorGUI.GetPropertyHeight(constantValueProp);
                if (constantValueProp.isExpanded && constantValueHeight > baseHeight)
                    return baseHeight + 2 + constantValueHeight;
            }
            return baseHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty useConstantProp = property.FindPropertyRelative("_useConstant");
            SerializedProperty constantValueProp = property.FindPropertyRelative("_constantValue");
            SerializedProperty referenceProp = property.FindPropertyRelative("_reference");

            Rect previousRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            label = EditorGUI.BeginProperty(position, label, property);
            Rect prefixRect = previousRect = new Rect(EditorGUI.PrefixLabel(previousRect, label));
            EditorGUI.EndProperty();


            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            Rect buttonRect = previousRect = new Rect(previousRect) { width = PopupStyle.fixedWidth + PopupStyle.margin.right, yMin = previousRect.yMin + PopupStyle.margin.top + 1 };
            useConstantProp.boolValue = !Convert.ToBoolean(EditorGUI.Popup(buttonRect, !useConstantProp.boolValue ? 1 : 0, _popupOptions, PopupStyle));

            Rect fieldValueRect = previousRect = new Rect(previousRect) { width = prefixRect.width, xMin = previousRect.xMax};
            if (useConstantProp.boolValue)
            {
                var constantValueHeight = EditorGUI.GetPropertyHeight(constantValueProp);
                if (constantValueHeight > EditorGUIUtility.singleLineHeight)
                {
                    if (GUI.Button(fieldValueRect, !constantValueProp.isExpanded ? "Show Value" : "Hide Value"))
                        constantValueProp.isExpanded = !constantValueProp.isExpanded;
                    if (constantValueProp.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        Rect constantValueRect = previousRect = new Rect(EditorGUI.IndentedRect(position)) { height = constantValueHeight, y = previousRect.yMax + 2 };
                        EditorGUI.indentLevel--;
                        EditorGUI.PropertyField(constantValueRect, constantValueProp);
                    }
                }
                else EditorGUI.PropertyField(fieldValueRect, constantValueProp, GUIContent.none);
            }
            else EditorGUI.PropertyField(fieldValueRect, referenceProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            EditorGUI.indentLevel = indent;
        }
    }
}

#endif