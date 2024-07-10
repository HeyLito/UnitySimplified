#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplifiedEditor.SpriteAnimator.Parameters
{
    [CustomPropertyDrawer(typeof(Parameter<>), true)]
    public class ParameterDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                height += 2;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("nameKeyword"));
                height += 2;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("lhsValue"));
                height += 2;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("comparer"));
                height += 2;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rhsValue"));
            }
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect previousRect = new Rect(position);
            Rect foldoutRect = previousRect = new Rect(previousRect) { height = EditorGUIUtility.singleLineHeight };
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            EditorGUI.indentLevel++;
            previousRect = EditorGUI.IndentedRect(foldoutRect);
            EditorGUI.indentLevel--;

            if (property.isExpanded)
            {
                SerializedProperty nameKeywordProp = property.FindPropertyRelative("nameKeyword");
                SerializedProperty lhsValueProp = property.FindPropertyRelative("lhsValue");
                SerializedProperty comparerProp = property.FindPropertyRelative("comparer");
                SerializedProperty rhsValueProp = property.FindPropertyRelative("rhsValue");

                previousRect.y += previousRect.height + 2;
                Rect nameKeywordRect = previousRect = new Rect(previousRect) { width = previousRect.width };
                previousRect.y += previousRect.height + 2;
                Rect lhsValueRect = previousRect = new Rect(previousRect);
                previousRect.y += previousRect.height + 2;
                Rect comparerRect = previousRect = new Rect(previousRect);
                previousRect.y += previousRect.height + 2;
                Rect rhsValueRect = previousRect = new Rect(previousRect);

                EditorGUI.PropertyField(nameKeywordRect, nameKeywordProp, new GUIContent("Name"));
                EditorGUI.PropertyField(lhsValueRect, lhsValueProp);
                EditorGUI.PropertyField(comparerRect, comparerProp);
                EditorGUI.PropertyField(rhsValueRect, rhsValueProp);
            }
        }
    }
}

#endif