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
        private readonly string[] _popupOptions = { "Use Constant", "Use Asset" };
        private GUIStyle _popupStyle;

        private GUIStyle PopupStyle => _popupStyle ??= new GUIStyle(GUI.skin.GetStyle("PaneOptions")) 
        {
            imagePosition = ImagePosition.ImageOnly
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUIUtility.singleLineHeight;
            SerializedProperty valueToggleProp = property.FindPropertyRelative("valueToggle");
            if (!valueToggleProp.boolValue)
                return baseHeight;

            SerializedProperty constantProp = property.FindPropertyRelative("constant");
            float constantPropHeight = EditorGUI.GetPropertyHeight(constantProp);
            if (constantProp.isExpanded && constantPropHeight > baseHeight)
                return baseHeight + 2 + constantPropHeight;
            return baseHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueToggleProp = property.FindPropertyRelative("valueToggle");
            SerializedProperty constantProp = property.FindPropertyRelative("constant");
            SerializedProperty referenceProp = property.FindPropertyRelative("reference");

            Rect previousRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            label = EditorGUI.BeginProperty(position, label, property);
            Rect prefixRect = previousRect = new Rect(EditorGUI.PrefixLabel(previousRect, label));
            EditorGUI.EndProperty();


            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            Rect buttonRect = previousRect = new Rect(previousRect) { width = PopupStyle.fixedWidth + PopupStyle.margin.right, yMin = previousRect.yMin + PopupStyle.margin.top + 1 };
            valueToggleProp.boolValue = !Convert.ToBoolean(EditorGUI.Popup(buttonRect, !valueToggleProp.boolValue ? 1 : 0, _popupOptions, PopupStyle));

            Rect fieldValueRect = previousRect = new Rect(previousRect) { width = prefixRect.width, xMin = previousRect.xMax};
            if (valueToggleProp.boolValue)
            {
                var constantPropHeight = EditorGUI.GetPropertyHeight(constantProp);
                if (constantPropHeight > EditorGUIUtility.singleLineHeight)
                {
                    if (GUI.Button(fieldValueRect, !constantProp.isExpanded ? "Show Value" : "Hide Value"))
                        constantProp.isExpanded = !constantProp.isExpanded;
                    if (constantProp.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        Rect constantPropRect = previousRect = new Rect(EditorGUI.IndentedRect(position)) { height = constantPropHeight, y = previousRect.yMax + 2 };
                        EditorGUI.indentLevel--;
                        EditorGUI.PropertyField(constantPropRect, constantProp);
                    }
                }
                else EditorGUI.PropertyField(fieldValueRect, constantProp, GUIContent.none);
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