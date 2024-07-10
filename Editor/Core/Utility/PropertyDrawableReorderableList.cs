#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnitySimplifiedEditor
{
    public class PropertyDrawableReorderableList
    {
        [NonSerialized]
        private readonly Dictionary<string, ReorderableList> _listsByProperties = new();

        public ReorderableList GetList(SerializedProperty property, Func<ReorderableList> defaultList)
        {
            if (property == null)
                throw new NullReferenceException($"Failed to get list, serialied property is NULL!");

            string propertyPath = GetPropertyPath(property);
            if (!_listsByProperties.TryGetValue(propertyPath, out var list) || !IsListValid(property, list))
                _listsByProperties[propertyPath] = list = defaultList.Invoke();
            return list;
        }

        public static ReorderableList DefaultListPreset(SerializedProperty property)
        {
            return new ReorderableList(property.serializedObject, property)
            {
                elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)),
                drawElementCallback = (rect, index, isActive, isFocused) => EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index)),
                drawHeaderCallback = (rect) =>
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    EditorGUI.PrefixLabel(rect, new GUIContent(property.displayName));
                    EditorGUI.indentLevel = indentLevel;
                }
            };
        }

        private string GetPropertyPath(SerializedProperty property) => property.serializedObject.targetObject.GetInstanceID() + "." + property.propertyPath;
        private bool IsListValid(SerializedProperty property, ReorderableList list)
        {
            if (list != null && list.serializedProperty != null)
            {
                try
                {
                    return list.serializedProperty.propertyPath == property.propertyPath;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
            else return false;
        }
    }
}

#endif