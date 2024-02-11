#if UNITY_EDITOR

using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator.Parameters;

namespace UnitySimplifiedEditor.SpriteAnimator.Parameters
{
    [CustomPropertyDrawer(typeof(ParameterComparer), true)]
    public class ParameterComparerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var exposedProperty = property.ExposeProperty(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, true);
            if (exposedProperty != null)
            {
                PropertyInfo optionsInfo = null;
                PropertyInfo selectionInfo = null;

                foreach (var propertyInfo in exposedProperty.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (optionsInfo != null && selectionInfo != null)
                        break;

                    if (optionsInfo == null)
                    {
                        if (propertyInfo.Name == "Options")
                            optionsInfo = propertyInfo;
                    }
                    else if (selectionInfo == null)
                    {
                        if (propertyInfo.Name == "Selection")
                            selectionInfo = propertyInfo;
                    }
                }

                if (optionsInfo != null && selectionInfo != null)
                {
                    GUIStyle popupStyle = new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter };
                    int selection = (int)selectionInfo.GetValue(exposedProperty);
                    int popupSelection = EditorGUI.Popup(position, label.text, selection, (string[])optionsInfo.GetValue(exposedProperty), popupStyle);
                    if (selection != popupSelection)
                    {
                        selectionInfo.SetValue(exposedProperty, popupSelection);
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    }
                }
                else EditorGUI.LabelField(position, "Missing Data In Comparer", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            }
            else EditorGUI.LabelField(position, "Missing Comparer", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
        }
    }
}

#endif